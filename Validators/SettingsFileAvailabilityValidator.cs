using System;
using System.IO;
using FluentValidation;

namespace MailMonitor.Validators
{
    public class SettingsFileAvailabilityValidator : AbstractValidator<string>
    {
        public SettingsFileAvailabilityValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;
            
            RuleFor(settingsFileFullPath => settingsFileFullPath)
                .Must(DirectoryExists)
                .WithMessage("Папка, содержащая файл настроек, не найдена.")
                .Must(FileExists)
                .WithMessage("Файл настроек не найден.")
                .Must(FileReadable)
                .WithMessage("Ошибка доступа к файлу или попытки чтения.");
        }

        private bool DirectoryExists(string filePath)
        {
            return Directory.Exists(Path.GetDirectoryName(filePath));
        }

        private bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        private bool FileReadable(string filePath)
        {
            try
            {
                File.ReadAllText(filePath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}