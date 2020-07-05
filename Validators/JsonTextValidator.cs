using System.IO;
using FluentValidation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MailMonitor.Validators
{
    public class JsonTextValidator : AbstractValidator<string>
    {
        public JsonTextValidator()
        {
            RuleFor(settingsFileFullPath => TryParseJson(settingsFileFullPath))
                .Equal(true)
                .WithMessage("Validator : Некорректный JSON-файл.");
        }

        private bool TryParseJson(string filePath)
        {
            try
            {
                JObject.Parse(File.ReadAllText(filePath));
                return true;
            }
            catch(JsonReaderException)
            {
                return false;
            }
        }
    }
}
