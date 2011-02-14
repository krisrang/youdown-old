using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Lemon
{
    public class ViewModelBase : NotifyPropertyChangedBase, IDataErrorInfo
    {
        public bool IsValid
        {
            get { return string.IsNullOrEmpty(Error); }
        }

        public string Error
        {
            get
            {
                var context = new ValidationContext(this, null, null);
                var results = new List<ValidationResult>();

                return !Validator.TryValidateObject(this, context, results)
                    ? string.Join(Environment.NewLine, results.Select(x => x.ErrorMessage))
                    : null;
            }
        }

        public string this[string propertyName]
        {
            get
            {
                var context = new ValidationContext(this, null, null)
                {
                    MemberName = propertyName
                };

                var results = new List<ValidationResult>();
                var value = GetType().GetProperty(propertyName).GetValue(this, null);

                return !Validator.TryValidateProperty(value, context, results)
                    ? string.Join(Environment.NewLine, results.Select(x => x.ErrorMessage))
                    : null;
            }
        }

        public static void OpenScreen(string screen)
        {
            GetShell().OpenScreen(screen);
        }

        public static IShell GetShell()
        {
            return Resolver.GetInstance<IShell>();
        }

        public static IOverlay GetOverlay(string name)
        {
            return Resolver.GetInstance<IOverlay>(name);
        }
    }
}