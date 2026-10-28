namespace IzinTakip.Api.Features.IzinBelgeleri;

public sealed record IzinBelgesiYukleRequest(IFormFile Dosya);

public sealed record IzinBelgesiResponse(int Id, string DosyaAdi, string IcerikTuru, long Boyut, DateTime OlusturmaTarihi);
