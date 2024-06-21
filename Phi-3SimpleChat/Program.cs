namespace Phi_3SimpleChat
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var SLMRunner = new SLMRunner();
            var promptBuilder = new PromptBuilder(Configuration.SystemPrompt);
            var messages = new List<SLMMessage>();

            // Init
            Console.WriteLine(@"Initializing SLM...");
            await SLMRunner.InitializeAsync();
            while (!SLMRunner.IsReady)
            {
                await Task.Delay(100);
            }

            // chat start 
            Console.WriteLine(@"Ask your question. Type an empty string to Exit.");


            // chat loop
            while (true)
            {
                // Get user question
                Console.WriteLine();
                Console.Write(@"Q: ");
                var userQ = Console.ReadLine();
                if (string.IsNullOrEmpty(userQ))
                {
                    break;
                }

                // Freeze and retrieve history
                var history = messages.ToList();

                // Log message from user
                messages.Add(new SLMMessage(userQ, DateTime.Now, SLMMessageType.User));

                // Start storing the new reply from the SLM
                var responseMessage = new SLMMessage(string.Empty, DateTime.Now, SLMMessageType.Assistant);
                messages.Add(responseMessage);

                // Ask with no memory.
                //var fullPrompt = promptBuilder.GetPrompt(userQ, null);

                // Ask with memory.
                var fullPrompt = promptBuilder.GetPrompt(userQ, history);

                // show phi3 response
                Console.Write(Environment.NewLine);
                Console.Write("Phi3: ");

                await foreach (var partialResult in SLMRunner.InferStreamingAsync(fullPrompt))
                {
                    Console.Write(partialResult);
                    responseMessage.Text += partialResult;
                }

                Console.WriteLine();
            }
        }
    }
}
