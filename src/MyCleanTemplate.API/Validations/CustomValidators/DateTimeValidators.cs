//using FluentValidation;

//namespace MyCleanTemplate.API.Validations.CustomValidators
//{
//    public static class DateTimeValidators
//    {
//        public static IRuleBuilderOptions<T, DateTime> AfterSunrise<T>(this IRuleBuilder<T, DateTime> ruleBuilder) =>
//            ruleBuilder.Must((_, dateTime, context) =>
//            {
//                var sunrise = TimeOnly.MinValue.AddHours(6);
//                var providedTime = TimeOnly.FromDateTime(dateTime);
//                context.MessageFormatter.AppendArgument("Sunrise", sunrise.ToString("h:mm:ss tt"));
//                return providedTime > sunrise;
//            }).WithMessage("{PropertyName} must be after {Sunrise}!");
//    }
//}
