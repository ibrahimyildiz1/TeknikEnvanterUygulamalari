using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperasyonYonetimSistemi
{
    using System;
    using System.Reflection;

    class Program
    {

        static void Main(string[] args)
        {
            int müşteriNumarası = 15000000;

            ÇalıştırmaMotoru.KomutÇalıştır("MuhasebeModulu", "MaaşYatır", new object[] { müşteriNumarası });

            ÇalıştırmaMotoru.KomutÇalıştır("MuhasebeModulu", "YıllıkÜcretTahsilEt", new object[] { müşteriNumarası });

            //istersek yukarıdaki gibi tek tek komutlar halinde çalıştırabiliriz.

            ÇalıştırmaMotoru.BekleyenİşlemleriGerçekleştir("MuhasebeModulu", KritiklikSeviyesi.Kirmizi, new object[] { müşteriNumarası });
            // ya da belirlenen kritiklik seviyesine uygun işlemleri öncelik sırasına göre çalıştırabiliyoruz.

            Console.Read();
        }
    }

    public class ÇalıştırmaMotoru
    {
        //injection yapılabilirdi fakat sanıyorum böyle bir beklentiniz yok.
        //IVeritabanıIslemleri _veritabanıİşlemleri;
        //public ÇalıştırmaMotoru(IVeritabanıIslemleri veritabanıİşlemleri)
        //{
        //    veritabanıİşlemleri = _veritabanıİşlemleri
        //}
        static Veritabanıİşlemleri veritabanıİşlemleri = new Veritabanıİşlemleri();
        public static object KomutÇalıştır(string modülSınıfAdı, string methodAdı, object[] inputs)
        {
            var modülSınıfı = ModülGetir(modülSınıfAdı);
            var modül = Activator.CreateInstance(modülSınıfı);
            var method = KomutBul(modülSınıfAdı, methodAdı);
            var result = method.Invoke(modül, inputs);

            //MusteriIslem tablosuna müşteri no operasyon ve diğer bilgilerle kayıt at kayıt at.
            return result;
        }

        public static void BekleyenİşlemleriGerçekleştir(string modülSınıfAdı, KritiklikSeviyesi öncelik, object[] inputs)
        {
            var bekleyenIslemler = veritabanıİşlemleri.OncelikDurumunaGoreIslemleriGetir(öncelik, modülSınıfAdı).Select(x => x.IslemAdi).ToList();

            if (bekleyenIslemler != null)
            {
                foreach (var islem in bekleyenIslemler)
                {
                    var komut = KomutBul(modülSınıfAdı, islem);
                    KomutÇalıştır(modülSınıfAdı, komut.Name, inputs);
                }
            }

        }

        private static Type ModülGetir(string modülAdı)
        {
            Assembly modüller = Assembly.GetExecutingAssembly();

            foreach (Type type in modüller.GetTypes())
            {
                if (modülAdı.Equals(type.Name))
                { return type; }
            }
            return null;
        }
        private static MethodInfo KomutBul(string modülAdı, string methodAdı)
        {
            var type = ModülGetir(modülAdı);
            var methodInfos = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);

            return methodInfos.Where(x => x.Name == methodAdı).SingleOrDefault();
        }
    }

    public class MuhasebeModulu
    {
        private void MaaşYatır(int müşteriNumarası)
        {
            // gerekli işlemler gerçekleştirilir.
            Console.WriteLine(string.Format("{0} numaralı müşterinin maaşı yatırıldı.", müşteriNumarası));
        }

        private void YıllıkÜcretTahsilEt(int müşteriNumarası)
        {
            // gerekli işlemler gerçekleştirilir.
            Console.WriteLine("{0} numaralı müşteriden yıllık kart ücreti tahsil edildi.", müşteriNumarası);
        }

        private void OtomatikÖdemeleriGerçekleştir(int müşteriNumarası)
        {
            // gerekli işlemler gerçekleştirilir.
            Console.WriteLine("{0} numaralı müşterinin otomatik ödemeleri gerçekleştirildi.", müşteriNumarası);
        }
        private void HesapÜcretiTahsilEt(int müşteriNumarası)
        {
            // gerekli işlemler gerçekleştirilir.
            Console.WriteLine("{0} numaralı müşterinin hesap ücreti tahsil edildi.", müşteriNumarası);
        }
    }

    // Bu arada siz tüm class ları program.cs te yazdığınız için ben de buradan devam ettim. Yoksa ayırmak gerekir tabi okunabilirlik ve modülerlik açısından 
    //public interface IVeritabanıIslemleri
    //{
    //    IEnumerable<Islem> OncelikDurumunaGoreIslemleriGetir(Oncelik oncelik);
    //    void MusteriOperasyonuEkle(MusteriIslem musteriOperasyonu);
    //}
    public class Veritabanıİşlemleri //: IVeritabanıIslemleri ne bildiğini öğrenmek istiyoruz dediğiniz için bunu da göstermek için ekledim sadece
    {
        public List<Islem> OncelikDurumunaGoreIslemleriGetir(KritiklikSeviyesi kritiklikSeviyesi, string modülSınıfıAdı)
        {
            List<Modul> modüller = new List<Modul>()
            {
                new Modul
                {
                    Id = 1,
                    ModulAdi = "MuhasebeModulu"
                }
            };

            List<Islem> islemler = new List<Islem>()
            {
                new Islem
                {
                    Id = 1,
                    IslemAdi = "OtomatikÖdemeleriGerçekleştir",
                    ModulId = 1,
                    KritiklikSeviyesi = KritiklikSeviyesi.Kirmizi,
                    Öncelik = 1
                },
                new Islem
                {
                    Id = 1,
                    IslemAdi = "HesapÜcretiTahsilEt",
                    ModulId = 1,
                    KritiklikSeviyesi = KritiklikSeviyesi.Kirmizi,
                    Öncelik = 2
                }
            };
            // 
            var modül = modüller.Find(x => x.ModulAdi.Equals(modülSınıfıAdı));// null check yapılabilir tabi ki burda. Static datalar olduğundan yapmadım. Ama farkında olduğumu belirtmek isterim.
            var result = islemler.Where(x => x.ModulId == modül.Id && x.KritiklikSeviyesi == kritiklikSeviyesi).OrderBy(x => x.Öncelik);
            return result.ToList();
        }
    }

    public class Musteri
    {
        public string MusteriNo { get; set; } //int Id de alabilir fakat unique olduğudan bunu kullanacağım uygulamada.
        public string Adi { get; set; }
        public string Soyadi { get; set; }
        public ICollection<MusteriIslem> MusteriIslemleri { get; set; }
    }
    public class Modul
    {
        public int Id { get; set; }
        public string ModulAdi { get; set; }
        public ICollection<Islem> Operasyonlar { get; set; }
    }
    public class Islem
    {
        public int Id { get; set; }
        public string IslemAdi { get; set; }
        public KritiklikSeviyesi KritiklikSeviyesi { get; set; } // int de olabilir fakat ef de bu konfigürasyon yapılabiliyor. Müşteri işlemi bazında da tanımlanabilir fakat ifade genel olduğundan burada değerlendirdim. tüm müşteriler için bekletilen işlemler mahiyetinde.
        public int Öncelik { get; set; }
        public int ModulId { get; set; }
        public Modul Modul { get; set; }
    }

    public class MusteriIslem
    {
        public int Id { get; set; }
        public string MusteriNo { get; set; }
        public Musteri Musteri { get; set; }
        public int IslemId { get; set; }
        public Islem Islem { get; set; }
        public DateTime GerceklesmeTarihi { get; set; }
    }

    public enum KritiklikSeviyesi
    {
        Kirmizi = 1,
        Sari = 2,
        Yesil = 3
    }
}
