namespace IzinTakip.Api.Features.ResmiTatiller;

public sealed record ResmiTatilKaydetRequest(DateOnly Tarih, string Ad, bool YarimGun);

public sealed record ResmiTatilResponse(int Id, DateOnly Tarih, string Ad, bool YarimGun);
