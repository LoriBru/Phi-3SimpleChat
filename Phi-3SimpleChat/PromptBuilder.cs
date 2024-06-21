using System.Diagnostics;

namespace Phi_3SimpleChat
{
    public class PromptBuilder
    {
        #region Private Fields

        private string _systemPrompt;

        #endregion Private Fields

        #region Public Constructors

        public PromptBuilder(string systemPrompt)
        {
            _systemPrompt = systemPrompt;
        }

        #endregion Public Constructors

        #region Public Methods

        public string GetPrompt(string userPrompt, List<SLMMessage>? history = null, string? context = null)
        {
            string fullPrompt = string.Empty;
            string extraContext = string.Empty;

            if (!string.IsNullOrEmpty(context))
            {
                extraContext = $"{Environment.NewLine}Use the following context:{Environment.NewLine}{context}";

                // Make sure RAG Context isn't too long (truncate it)
                var maxAllowedContextLength = Configuration.MaxPromptLength - _systemPrompt.Length - userPrompt.Length - 46; // the last number factors in the chat prompt format used
                if (extraContext.Length > maxAllowedContextLength)
                {
                    Console.WriteLine("RAG Context too long, truncating it...");
                    extraContext = extraContext.Substring(0, maxAllowedContextLength);
                }
            }

            // Build prompt conversation
            if (history != null)
            {
                fullPrompt = $"<|system|>{Environment.NewLine}{_systemPrompt}{extraContext}<|end|>{Environment.NewLine}";

                // Add history
                foreach (var message in history)
                {
                    fullPrompt += $"<|{message.Type.ToString().ToLower()}|>{message.Text}<|end|>{Environment.NewLine}";
                }

                // Add current query 
                fullPrompt += $"<|user|>{userPrompt}<|end|>{Environment.NewLine}<|assistant|>";
            }
            else
            {
                fullPrompt = $"<|system|>{Environment.NewLine}{_systemPrompt}{extraContext}<|end|>{Environment.NewLine}<|user|>{userPrompt}<|end|>{Environment.NewLine}<|assistant|>";
            }

            Debug.WriteLine($"{Environment.NewLine}Prompt length: {fullPrompt.Length}");
            Debug.WriteLine($"Full prompt:{Environment.NewLine}--------------------------------------{Environment.NewLine}{fullPrompt}{Environment.NewLine}--------------------------------------{Environment.NewLine}");

            return fullPrompt;
        }

        #endregion Public Methods
    }
}
