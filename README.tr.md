<div align="center">

<img src="https://img.shields.io/badge/sürüm-1.3.2-0078D4?style=flat-square" alt="Sürüm"/>
<img src="https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square" alt=".NET"/>
<img src="https://img.shields.io/badge/platform-Windows-0078D4?style=flat-square" alt="Platform"/>
<img src="https://img.shields.io/badge/lisans-MIT-10B981?style=flat-square" alt="Lisans"/>

# OpenSourceHub

**Profesyonel GitHub Zeka Platformu**

Analiz Et. Keşfet. Daha İyi İnşa Et.

[v1.3.2 İndir](https://github.com/X1NPAR1/OpenSourceHub/releases/latest) · [Hata Bildir](https://github.com/X1NPAR1/OpenSourceHub/issues) · [Özellik İste](https://github.com/X1NPAR1/OpenSourceHub/issues)

</div>

---

## Genel Bakış

OpenSourceHub, GitHub depolarını analiz etmek için geliştirilmiş profesyonel bir Windows masaüstü uygulamasıdır. GitHub API verileri, yapay zeka destekli içgörüler, güvenlik değerlendirmeleri ve kapsamlı raporlamayı modern karanlık temalı bir WPF arayüzünde birleştirir.

**.NET 10**, **WPF** ve **Clean Architecture** ile geliştirilmiş olan uygulama, açık kaynak projeleri değerlendirmek, karşılaştırmak ve izlemek için ihtiyacınız olan her şeyi sunar.

---

## Özellikler

| Özellik | Açıklama |
|---------|----------|
| **Depo Analizi** | 6 boyutlu sağlık puanlaması: Aktivite, Bakım, Güvenlik, Topluluk, Popülerlik, Sağlık |
| **AI İçgörüleri** | OpenAI ve Ollama destekli özetler, benimseme önerileri, iyileştirme tavsiyeleri |
| **Güvenlik Merkezi** | Bakım, lisans ve aktivite boyutlarında risk tespiti |
| **Trend Keşfedici** | Dile ve zaman dilimine göre yükselen depoları keşfedin |
| **Karşılaştırma** | 4 depoya kadar yan yana karşılaştırma |
| **Organizasyonlar** | GitHub organizasyonlarını, üyeleri ve depoları keşfedin |
| **Favoriler** | Depoları yer imleri ekleyin ve notlar alın |
| **Raporlar** | PDF, Markdown ve HTML formatlarında rapor dışa aktarın |
| **Günlükler** | Filtreleme ve dışa aktarma destekli uygulama olay günlüğü |
| **Ayarlar** | Tema, dil (TR/EN/RU), AI sağlayıcı, önbellek ve bildirim yapılandırması |

---

## Başlangıç

### Gereksinimler

- Windows 10 x64 veya üzeri
- [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)
- GitHub Kişisel Erişim Token'ı

### Kurulum

1. [Releases](https://github.com/X1NPAR1/OpenSourceHub/releases/latest) sayfasından son sürümü indirin
2. ZIP dosyasını açın
3. `OpenSourceHub.exe` dosyasını çalıştırın

### GitHub Token Kurulumu

1. **GitHub → Ayarlar → Geliştirici Ayarları → Kişisel Erişim Token'ları** sayfasına gidin
2. Aşağıdaki kapsamlarla yeni token oluşturun:
   ```
   repo, read:org, read:user, user:email
   ```
3. Token'ı kopyalayın (`ghp_` veya `github_pat_` ile başlar)
4. Giriş ekranına yapıştırın ve **GitHub ile Giriş Yap** düğmesine tıklayın

---

## Kaynaktan Derleme

### Önkoşullar

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows 10 x64 veya üzeri
- Visual Studio 2022+ veya JetBrains Rider

### Adımlar

```bash
git clone https://github.com/X1NPAR1/OpenSourceHub.git
cd OpenSourceHub
dotnet restore
dotnet build
dotnet run --project src/OpenSourceHub.UI
```

### Yayınlama (Release Build)

```bash
dotnet publish src/OpenSourceHub.UI/OpenSourceHub.UI.csproj \
  -c Release -r win-x64 --self-contained false \
  -o ./release/output
```

---

## Mimari

```
OpenSourceHub/
├── src/
│   ├── OpenSourceHub.Domain          # Varlıklar, enum'lar, arayüzler
│   ├── OpenSourceHub.Application     # Kullanım senaryoları, uygulama mantığı
│   ├── OpenSourceHub.Infrastructure  # GitHub API, AI servisleri, SQLite, EF Core
│   ├── OpenSourceHub.Localization    # TR / EN / RU dil stringleri
│   ├── OpenSourceHub.Reporting       # PDF / Markdown / HTML üretimi
│   └── OpenSourceHub.UI              # WPF masaüstü uygulaması (MVVM)
└── tests/
    └── OpenSourceHub.Tests           # xUnit birim ve entegrasyon testleri
```

**Desenler:** Clean Architecture · MVVM (CommunityToolkit.Mvvm) · Bağımlılık Enjeksiyonu · Repository Deseni

---

## AI Yapılandırması

### OpenAI (Bulut)
1. **Ayarlar → AI → Sağlayıcı → OpenAI** seçin
2. OpenAI API anahtarınızı girin (`sk-...`)
3. Model belirleyin (varsayılan: `gpt-4o-mini`)

### Ollama (Yerel)
1. [Ollama](https://ollama.com/) kurulumunu yapın
2. Model indirin: `ollama pull llama3.2`
3. **Ayarlar → AI → Sağlayıcı → Ollama** seçin
4. Endpoint ve model adını belirleyin

---

## Yerelleştirme

Uygulama, **Ayarlar → Görünüm → Dil** kısmından anında değiştirilebilen üç dili destekler:

| Dil | Kod |
|-----|-----|
| English | EN |
| Türkçe | TR |
| Русский | RU |

---

## Lisans

MIT Lisansı — ayrıntılar için [LICENSE](LICENSE) dosyasına bakın.

---

## Katkıda Bulunma

Katkılar memnuniyetle karşılanır. Pull request göndermeden önce önerilen değişikliği tartışmak için lütfen bir issue açın.

---

<div align="center">
❤️ ile geliştirildi — <a href="https://github.com/X1NPAR1">X1NPAR1</a> · XinPari Software · Sürüm 1.3.2
</div>
