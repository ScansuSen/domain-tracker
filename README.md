# Domain Tracker

Uygulama üzerinden bir domainin kullanılabilir olup olmadığı kontrol edilebilir. Sorgulanan domainler favorilere eklenebilir ve daha sonra kullanılabilirlik durumu, son kontrol tarihi ve varsa bitiş tarihi görüntülenebilir. Favoriye eklenen bir domain istenildiğinde tekrar sorgulanabilir.

Domain bilgileri case dokümanında verilen RDAP servisi üzerinden alınıyor:

`https://rdap.nicproxy.com/domain/{domain_name}/`

Servisten `404` dönmesi domainin kullanılabilir olduğu, `2xx` bir cevap dönmesi ise domainin kayıtlı olduğu anlamına geliyor. Kayıtlı domainlerin bitiş tarihi RDAP response içerisindeki `expiration` event'inden alınıyor.

Proje monorepo olarak hazırlandı:

* `apps/api` → .NET 8 Web API
* `apps/web` → React + Vite frontend

Frontend case kapsamında opsiyoneldi. Backend akışını daha rahat gösterebilmek ve uygulamayı uçtan uca kullanılabilir hale getirmek için basit bir arayüz de ekledim.

## Docker ile Çalıştırma

Projeyi çalıştırmanın en kolay yolu Docker Compose kullanmak. Local ortamda ayrıca .NET SDK, Node veya SQL Server kurmaya gerek yok.

Repo'nun ana dizininde:

```bash
docker compose up --build
```

komutunu çalıştırmak yeterli.

Docker Compose ile SQL Server, API ve frontend birlikte ayağa kalkar.

Uygulama:

`http://localhost:3000`

Swagger:

`http://localhost:8080/swagger`

adresinden açılabilir.

API başlarken bekleyen EF Core migration'ları otomatik olarak uygulanır. Bu nedenle ayrıca `dotnet ef database update` çalıştırmaya gerek yoktur.

Container'ları durdurmak için:

```bash
docker compose down
```

SQL Server volume'unu da silmek için:

```bash
docker compose down -v
```

---

# Backend

## Kullanılan Teknolojiler

* .NET 8 / ASP.NET Core Web API
* Entity Framework Core 8
* SQL Server
* Autofac
* AutoMapper
* FluentValidation
* JWT Authentication
* NLog
* xUnit + Moq
* Docker

## Case Gereksinimleri

Case'de istenen temel işlemlerin tamamı API tarafında bulunuyor:

| Method | Endpoint                      | Auth  | Açıklama                               |
| ------ | ----------------------------- | ----- | -------------------------------------- |
| POST   | `/api/auth/register`          | Hayır | Yeni kullanıcı oluşturur               |
| POST   | `/api/auth/login`             | Hayır | Kullanıcı girişi yapar ve JWT döner    |
| GET    | `/api/domains/check?name=`    | Hayır | Domaini RDAP üzerinden kontrol eder    |
| GET    | `/api/favorites`              | JWT   | Kullanıcının favorilerini getirir      |
| POST   | `/api/favorites`              | JWT   | Domaini favorilere ekler               |
| DELETE | `/api/favorites/{id}`         | JWT   | Favoriyi siler                         |
| POST   | `/api/favorites/{id}/refresh` | JWT   | Domain bilgilerini tekrar kontrol eder |

Case dokümanında authentication ayrıca istenmiyordu. Ancak favorilerin kullanıcıya özel olması gerektiğini düşündüğüm için basit bir register/login akışı ekledim. Böylece her kullanıcı kendi favori listesini yönetebiliyor.

JWT gerektiren endpoint'lerde token:

```text
Authorization: Bearer <token>
```

şeklinde gönderiliyor.

API cevaplarında ortak bir result yapısı kullanılıyor:

```json
{
  "success": true,
  "statusCode": 200,
  "messages": [],
  "data": {}
}
```

## Backend'i Docker Olmadan Çalıştırma

### Gereksinimler

* .NET 8 SDK
* SQL Server
* İsteğe bağlı olarak `dotnet-ef`

Öncelikle `apps/api/DomainTracker/DomainTracker.API/appsettings.json` içerisindeki connection string kendi SQL Server bağlantınıza göre düzenlenmelidir.

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=DomainTracker;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Ardından repo'nun ana dizininden API çalıştırılabilir:

```bash
dotnet run --project apps/api/DomainTracker/DomainTracker.API
```

Migration'lar uygulama başlarken otomatik uygulanır.

Manuel olarak çalıştırmak isterseniz:

```bash
dotnet ef database update --project apps/api/DomainTracker/DomainTracker.DataAccess --startup-project apps/api/DomainTracker/DomainTracker.API
```

Development ortamında Swagger `/swagger` adresinden kullanılabilir.

## Testler

Backend tarafında xUnit ve Moq ile unit testler bulunuyor.

Testleri çalıştırmak için:

```bash
dotnet test apps/api/DomainTracker/DomainTracker.Tests
```

Business servislerinin bağımlılıkları mocklandığı için testler SQL Server veya RDAP servisine ihtiyaç duymadan çalışabilir.

## Bazı Tasarım Kararları

Domain bilgilerini doğrudan kullanıcıya bağlamak yerine `Domains` ve `FavoriteDomains` yapılarını ayırdım. `Domains` domainin kendisini ve son sorgu sonucunu tutarken, `FavoriteDomains` kullanıcının hangi domainleri takip ettiğini tutuyor. Böylece iki kullanıcı aynı domaini favorilerine eklediğinde aynı domain bilgisi veritabanında tekrar tekrar oluşturulmuyor.

Bir domain favorilere eklenirken RDAP üzerinden tekrar kontrol ediliyor. Bunun nedeni daha önce sorgulanmış bir domainin eski bilgisiyle favorilere eklenmesini engellemek.

RDAP tarafında da yalnızca `404` cevabını "domain kullanılabilir" olarak değerlendiriyorum. Bağlantı problemi veya beklenmeyen bir HTTP cevabı geldiğinde domaini yanlışlıkla kullanılabilir göstermek yerine API hata döndürüyor.

Data access tarafında ortak `GetById`, `Add`, `Update` ve `Delete` işlemleri generic repository içerisinde bulunuyor. Böylece generic repository'yi her entity'nin özel ihtiyacını karşılamaya çalışan büyük bir yapıya dönüştürmemeye çalıştım.

Beklenen validation, authentication, not found veya conflict gibi durumlarda servisler uygun result'ı dönüyor. Beklenmeyen hatalar ise merkezi exception handler tarafından yakalanıyor.

## Production İçin Notlar / Neleri Farklı Yapardım?

Bu proje bir coding case olduğu için bazı noktaları bilinçli olarak daha basit tuttum. Buradaki amacım production seviyesindeki bütün olası ihtiyaçları projeye eklemek yerine, case'in istediği işlevleri temiz ve anlaşılır bir yapıyla tamamlamaktı.

Authentication bunun örneklerinden biri. Kullanıcıları birbirinden ayırabilmek için JWT tabanlı basit bir login/register akışı yeterliydi. Mevcut yapıda access token belirli bir süre geçerli ve süresi dolduğunda kullanıcı tekrar giriş yapıyor. Gerçek bir projede ihtiyaca göre refresh token ve token iptali gibi konuları da ele alırdım. JWT bilgilerini database üzerinde saklardım. Signing key gibi hassas değerleri de repository içerisinde tutmak yerine environment variable veya kullanılan ortama uygun bir secret management çözümüyle yönetirdim.

CORS tarafında API'ye ayrıca bir policy eklemedim. Development ortamında frontend istekleri Vite proxy üzerinden, Docker ortamında ise nginx üzerinden API'ye yönlendiriliyor. Bu nedenle tarayıcı frontend ve API ile farklı origin'ler üzerinden haberleşmiyor. Frontend ve API'nin ayrı origin'lerde deploy edildiği bir production ortamında ise izin verilen origin, method ve header'ları açıkça belirleyen bir CORS policy tanımlardım.

RDAP servisi şu anda her domain kontrolünde çağrılıyor. Gerçek kullanımda aynı domainin kısa aralıklarla tekrar sorgulanması hem gereksiz trafik oluşturabilir. Böyle bir durumda son kontrol zamanını dikkate alan kısa süreli bir cache kullanmayı tercih ederdim.

Test tarafında şu an business servislerini kapsayan unit testler bulunuyor. Daha kapsamlı bir projede bunlara integration testler de eklerdim. Özellikle routing, authentication middleware, model binding, database işlemleri ve API response'larının birlikte doğru çalıştığını görmek için `WebApplicationFactory` üzerinden API seviyesinde testler yazılabilir.

Favori listesi case kapsamında küçük olacağı için tek seferde dönüyor. Gerçek kullanımda veri miktarı büyüdüğünde pagination eklemek daha doğru olurdu. Benzer şekilde dışarıya açık domain sorgulama endpoint'i için rate limiting ve servisin durumunu takip edebilmek için health check gibi operasyonel ihtiyaçları da production aşamasında değerlendirirdim.

---

# Frontend

## Kullanılan Teknolojiler

* React 19
* TypeScript
* Vite
* React Router
* Axios
* Bootstrap 5
* Bootstrap Icons

Case'de frontend opsiyonel olarak belirtilmişti. Backend'deki akışları kullanılabilir bir arayüz üzerinden göstermek için temel bir frontend hazırladım.

Uygulamada:

* Domain sorgulama
* Favorilere ekleme
* Favorileri görüntüleme
* Domain bilgisini yenileme
* Favorilerden silme
* Login / Register

akışları bulunuyor.

## Frontend'i Docker Olmadan Çalıştırma

Node.js 20+ gereklidir.

Frontend klasörüne geçip bağımlılıkları kurun:

```bash
cd apps/web
npm install
```

`.env.example` dosyasını `.env` olarak kopyalayın:

```bash
cp .env.example .env
```

Ardından development server'ı başlatın:

```bash
npm run dev
```

Bu sırada backend'in de çalışıyor olması gerekir.

Frontend development ortamında `/api` isteklerini Vite üzerinden backend'e proxy'ler. Docker ortamında aynı işi nginx yapar. Böylece iki ortamda da frontend tarafındaki API çağrıları aynı şekilde `/api` üzerinden yapılabilir.

## Son Not

Case'i geliştirirken önceliğim istenen fonksiyonları tamamlamak, kodu okunabilir tutmak ve backend tarafındaki kararları mümkün olduğunca basit bir yapıyla göstermekti.

Authentication ve Docker desteği case'in temel gereksinimlerinin biraz dışına çıkıyor. Bunları projeyi gereksiz yere büyütmeden, uygulamanın bütün olarak çalışmasını ve daha rahat değerlendirilebilmesini sağlamak için ekledim.
