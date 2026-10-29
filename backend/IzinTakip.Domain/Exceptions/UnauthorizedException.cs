namespace IzinTakip.Domain.Exceptions;

public sealed class UnauthorizedException(string message) : IzinTakipException(message);
