namespace IzinTakip.Domain.Exceptions;

public sealed class NotFoundException(string message) : IzinTakipException(message);
