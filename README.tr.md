# OpenSourceHub

<div align="center">

![OpenSourceHub](https://img.shields.io/badge/OpenSourceHub-v1.1.5-0078D4?style=for-the-badge&logo=github)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)
![Lisans](https://img.shields.io/badge/Lisans-MIT-green?style=for-the-badge)
![Platform](https://img.shields.io/badge/Platform-Windows-blue?style=for-the-badge)

**Analiz Et. Keşfet. Daha İyi İnşa Et.**

*Windows için profesyonel GitHub zeka platformu*

</div>

---

## Genel Bakış

OpenSourceHub, derin GitHub depo analizi, yapay zeka destekli içgörüler, güvenlik taraması, trend keşfi ve profesyonel raporlama sunan premium bir Windows masaüstü uygulamasıdır — tüm bunlar modern Fluent Design arayüzünde.

**.NET 10**, **WPF** ve **Clean Architecture** ile inşa edilmiş olup premium bir masaüstü deneyiminde kurumsal kalitede yazılım sunar.

## Özellikler

| Özellik | Açıklama |
|---------|----------|
| 📊 **Gösterge Paneli** | GitHub profilinizin, depolarınızın, yıldızlarınızın ve aktivitenizin gerçek zamanlı genel görünümü |
| 🔍 **Depo Analizi** | 6 boyutta derin sağlık puanlaması: Sağlık, Bakım, Güvenlik, Topluluk, Popülerlik, Aktivite |
| 🔥 **Trend Keşfedici** | Dile ve döneme göre trend depoları keşfedin |
| 🏢 **Organizasyon Analitiği** | GitHub organizasyonlarını, üyeleri ve depoları analiz edin |
| ⚖️ **Depo Karşılaştırma** | 4'e kadar depoyu yan yana karşılaştırın |
| 🛡️ **Güvenlik Merkezi** | Güvenlik riski taraması ve AI risk raporları |
| 🤖 **AI Analizleri** | OpenAI ve Ollama destekli özetler, benimseme önerileri ve iyileştirme önerileri |
| 📄 **Raporlar** | Profesyonel PDF, Markdown ve HTML rapor dışa aktarımı |
| ⭐ **Favoriler** | Depoları kaydedin ve notlar ekleyin |
| 📋 **Uygulama Kayıtları** | Dışa aktarma destekli yapılandırılmış günlük kaydı |
| ⚙️ **Ayarlar** | Temalar, dil, AI sağlayıcılar ve önbellek için kapsamlı yapılandırma |

## Mimari

```
OpenSourceHub (Çözüm)
├── OpenSourceHub.UI              ← WPF, Görünümler, ViewModel'ler (MVVM)
├── OpenSourceHub.Application     ← Kullanım Durumları, Servis Arayüzleri
├── OpenSourceHub.Domain          ← Entity'ler, Enum'lar, Domain Arayüzleri
├── OpenSourceHub.Infrastructure  ← EF Core, GitHub API, AI Servisleri
├── OpenSourceHub.Localization    ← TR/EN/RU Yerelleştirme Yöneticisi
├── OpenSourceHub.Reporting       ← PDF, Markdown, HTML Raporlar
└── OpenSourceHub.Tests           ← Birim ve Entegrasyon Testleri
```

## Kurulum

### Gereksinimler
- Windows 10 (1903+) veya Windows 11
- .NET 10 Runtime
- GitHub Kişisel Erişim Token'ı

### Seçenek 1: Taşınabilir (Önerilen)
1. `OpenSourceHub-v1.1.5-portable.zip` dosyasını indirin
2. İstediğiniz klasöre çıkartın
3. `OpenSourceHub.exe` dosyasını çalıştırın

### Seçenek 2: Yükleyici
1. `OpenSourceHub-v1.1.5-Setup.exe` dosyasını indirin
2. Yükleyiciyi çalıştırın
3. Kurulum sihirbazını takip edin

## Yapılandırma

### GitHub Token'ı
1. GitHub → Ayarlar → Geliştirici ayarları → Kişisel erişim token'ları
2. `repo`, `read:org`, `read:user`, `user:email` kapsamlarıyla yeni bir token oluşturun
3. Token'ı OpenSourceHub'a giriş yaparken girin

## Lisans

MIT Lisansı — ayrıntılar için [LICENSE](LICENSE) dosyasına bakın.

---

<div align="center">

**© 2026 XinPari Software** | Sürüm 1.1.5

</div>
