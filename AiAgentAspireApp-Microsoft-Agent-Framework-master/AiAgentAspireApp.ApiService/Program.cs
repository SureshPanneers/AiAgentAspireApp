
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.AddOpenAIClient("ollama-chat-client", settings =>
{
    settings.EnableSensitiveTelemetryData = true;
})
   .AddChatClient();

builder.AddAIAgent("Writer", (sp, key) =>
{
    var chatClient = sp.GetRequiredService<IChatClient>();
    return new ChatClientAgent(
        chatClient,
        name: key,
        instructions:
            """
            You are a creative writing assistant who crafts vivid, 
            well-structured stories with compelling characters based on user prompts, 
            and formats them after writing.
            """,
        tools: [  AIFunctionFactory.Create(GetAuthor),
            AIFunctionFactory.Create(FormatStory)]
        );

    [Description("Gets the author of the story.")]
    string GetAuthor() => "Jack Torrance";

    [Description("Formats the story for display.")]
    string FormatStory(string title, string author, string story) =>
        $"Title: {title}\nAuthor: {author}\n\n{story}";
});


builder.AddAIAgent(
    name: "Editor",
    instructions:
        """
        You are an editor who improves a writer’s draft by providing 4–8 concise recommendations and 
        a fully revised Markdown document, focusing on clarity, coherence, accuracy, and alignment.
        """);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/agent/chat", async (
    [FromKeyedServices("Writer")] AIAgent writer,
    [FromKeyedServices("Editor")] AIAgent editor,
    HttpContext context,
    string prompt) =>
{
    Workflow workflow =
        AgentWorkflowBuilder
            .CreateGroupChatBuilderWith(agents =>
                new AgentWorkflowBuilder.RoundRobinGroupChatManager(agents)
                {
                    MaximumIterationCount = 2
                })
            .AddParticipants(writer, editor)
            .Build();

    AIAgent workflowAgent = await workflow.AsAgentAsync();

    context.Response.ContentType = "text/plain";

    await foreach (var chunk in workflowAgent.RunStreamingAsync(prompt))
    {
        if (!string.IsNullOrEmpty(chunk?.Text))
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(chunk.Text);
            await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
            await context.Response.Body.FlushAsync();
        }
    }
});

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
