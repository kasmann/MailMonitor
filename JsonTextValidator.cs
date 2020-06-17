using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentValidation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MailMonitor
{
    class JsonTextValidator : AbstractValidator<string>
    {
        public JsonTextValidator()
        {
            RuleFor(settingsFileFullPath => File.Exists(settingsFileFullPath))
                .Equal(true)
                .WithMessage($"Validator : Файл настроек не найден.")
                .DependentRules(() =>
                {
                    RuleFor(settingsFileFullPath => ReadContent(settingsFileFullPath))
                        .NotEqual(string.Empty)
                        .WithMessage($"Validator : Файл настроек пуст.")
                        .DependentRules(() =>
                        {
                            RuleFor(settingsFileFullPath => ParseJson(settingsFileFullPath))
                                .Equal(true)
                                .WithMessage("Validator : Некорректный JSON-файл.");
                        }
                        );
                }
                );
        }

        private string ReadContent(string filePath)
        {
            var content = File.ReadAllText(filePath);
            return content;
        }

        private bool ParseJson(string filePath)
        {
            try
            {
                JObject.Parse(ReadContent(filePath));
                return true;
            }
            catch(JsonReaderException)
            {
                return false;
            }
        }
    }
}
