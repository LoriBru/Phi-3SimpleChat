using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.ML.OnnxRuntimeGenAI;

namespace Phi_3SimpleChat
{
    public class SLMRunner : IDisposable
    {
        #region Private Fields

        private readonly string ModelDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "onnx-models", "phi3-directml-int4-awq-block-128");

        private Model? _model = null;
        private Microsoft.ML.OnnxRuntimeGenAI.Tokenizer? _tokenizer = null;

        #endregion Private Fields

        #region Public Properties

        [MemberNotNullWhen(true, nameof(_model), nameof(_tokenizer))]
        public bool IsReady => _model != null && _tokenizer != null;

        #endregion Public Properties

        #region Public Events

        public event EventHandler? ModelLoaded = null;

        #endregion Public Events

        #region Public Methods

        public void Dispose()
        {
            _model?.Dispose();
            _tokenizer?.Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public async IAsyncEnumerable<string> InferStreamingAsync(string prompt, [EnumeratorCancellation] CancellationToken ct = default)
        {
            if (!IsReady)
            {
                throw new InvalidOperationException("Model is not ready");
            }

            var generatorParams = new GeneratorParams(_model);

            // 5.1) Tokenize the input text
            var sequences = _tokenizer.Encode(prompt);

            generatorParams.SetSearchOption("max_length", Configuration.MaxPromptLength);
            //generatorParams.SetSearchOption("past_present_share_buffer", false);
            generatorParams.SetInputSequences(sequences);
            generatorParams.TryGraphCaptureWithMaxBatchSize(1);

            using var tokenizerStream = _tokenizer.CreateStream();
            using var generator = new Generator(_model, generatorParams);
            StringBuilder stringBuilder = new();

            // 5.2) Generate the output text, streaming the results
            Stopwatch stopwatch = Stopwatch.StartNew();
            int tokenCount = 0;
            while (!generator.IsDone())
            {
                string part;
                try
                {
                    if (ct.IsCancellationRequested)
                    {
                        break;
                    }

                    await Task.Delay(0, ct).ConfigureAwait(false);
                    generator.ComputeLogits();
                    generator.GenerateNextToken();

                    tokenCount++;
                    if (stopwatch.Elapsed.TotalSeconds >= 1)
                    {
                        Debug.WriteLine($"Tokens per second: {tokenCount / stopwatch.Elapsed.TotalSeconds}");
                        stopwatch.Restart();
                        tokenCount = 0;
                    }

                    // 5.3) Decode the generated token
                    part = tokenizerStream.Decode(generator.GetSequence(0)[^1]);
                    stringBuilder.Append(part);
                    if (stringBuilder.ToString().Contains("<|end|>")
                        || stringBuilder.ToString().Contains("<|user|>")
                        || stringBuilder.ToString().Contains("<|system|>")
                        || stringBuilder.ToString().Contains("["))
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    break;
                }

                yield return part;
            }
        }

        public Task InitializeAsync(CancellationToken ct = default)
        {
            return Task.Run(() =>
            {
                var sw = Stopwatch.StartNew();
                _model = new Model(ModelDir);
                _tokenizer = new Microsoft.ML.OnnxRuntimeGenAI.Tokenizer(_model);
                sw.Stop();
                Debug.WriteLine($"Model loading took {sw.ElapsedMilliseconds} ms");
                ModelLoaded?.Invoke(this, EventArgs.Empty);
            }, ct);
        }

        #endregion Public Methods
    }
}
