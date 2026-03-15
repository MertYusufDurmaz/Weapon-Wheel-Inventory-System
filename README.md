# Weapon-Wheel-Inventory-System
Weapon Wheel & Inventory System
Bu modül, oyuncunun topladığı eşyaları (Fener, Anahtarlar, Günlük) havuzlama (Object Pooling) mantığıyla saklayan ve radyal bir arayüz (Weapon Wheel) ile hızlı erişim sağlayan bir envanter yöneticisidir.

Özellikler:

Performanslı UI Etkileşimi: Ağır Update döngüleri yerine Unity'nin yerleşik IPointerEnterHandler olaylarını kullanarak, fare takibini sıfır performans kaybıyla yönetir.

Object Pooling (Nesne Havuzlama): Toplanan eşyalar yok edilmez; sahne dışında pasif bir ItemPool transformunda tutulur, böylece kuşanma/bırakma işlemlerinde yeniden Instantiate yükü yaratmaz.

Güvenli Sahne Temizliği: Save/Load işlemlerinde eski objelerin kalıntı bırakmasını engellemek için özel ClearInventoryForLoading metodu barındırır.
