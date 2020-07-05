using System.IO;
using FluentValidation;

namespace MailMonitor.Validators
{
    public class FileNotEmptyValidator : AbstractValidator<string>
    {
        public FileNotEmptyValidator()
        {
            RuleFor(settingsFileFullPath => ReadContent(settingsFileFullPath))
                .NotEqual(string.Empty)
                .WithMessage($"Validator : Файл настроек пуст.");
        }

        private string ReadContent(string filePath)
        {
            var content = File.ReadAllText(filePath);
            return content;
        }
    }
}