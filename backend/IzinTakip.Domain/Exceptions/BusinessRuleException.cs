namespace IzinTakip.Domain.Exceptions;

public sealed class BusinessRuleException(string message) : IzinTakipException(message);
