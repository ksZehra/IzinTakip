namespace IzinTakip.Domain.Exceptions;

public sealed class ForbiddenException(string message) : IzinTakipException(message);
