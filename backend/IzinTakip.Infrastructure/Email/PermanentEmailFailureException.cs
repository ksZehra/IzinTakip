namespace IzinTakip.Infrastructure.Email;

public sealed class PermanentEmailFailureException(string message, Exception? innerException = null) : Exception(message, innerException);
