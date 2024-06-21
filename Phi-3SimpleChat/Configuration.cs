namespace Phi_3SimpleChat
{
    public static class Configuration
    {
        public static string SystemPrompt { get; set; } = "You are an AI assistant that helps people find information. ";

        public static int MaxPromptLength { get; set; } = 4096;
    }
}
