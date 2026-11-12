<div dir="rtl" lang="ar">

# التثبيت

اللغات: [Italiano](Installazione.md) | [English](en-Installation.md) | [Español](es-Installation.md) | [简体中文](zh-Installation.md) | [हिन्दी](hi-Installation.md) | [العربية](ar-Installation.md) | [Português](pt-Installation.md) | [বাংলা](bn-Installation.md) | [Русский](ru-Installation.md) | [اردو](ur-Installation.md)

## المتطلبات

- Minecraft Java **26.3** أو **26.4 Snapshot 7** بحسب ملف JAR المختار.
- Fabric Loader متوافق مع اللعبة.
- Fabric API لإصدار Minecraft نفسه.
- Java 25 أو أحدث.

## خطوات التثبيت

1. ثبّت Fabric Loader للإصدار المختار.
2. أضف Fabric API المتوافق إلى `mods`.
3. انسخ ملف JAR **واحدًا فقط** من BetterVanilla+ 1.1.0 إلى `mods`.
4. شغّل Minecraft بملف تعريف Fabric المطابق.

اختر أحد التنزيلين أدناه؛ لا تثبتهما معًا في ملف التعريف نفسه.

## الخادم والعملاء

لاستخدام المصاعد ورؤية العناصر المخصصة، ثبّت المود على الخادم وكل العملاء. في اللعب الفردي يستخدم الخادم المدمج تثبيت ملف التعريف المحلي نفسه. يجب وجود Fabric API في البيئات نفسها.

تعمل الأوامر وتعديلات القرويين على الخادم. جسيمات نشاط المصاعد تأثير على العميل، يراه من ثبّت المود.

## بناء المشروع

مع JDK 25، نفّذ `gradlew.bat build` على Windows. يُنشأ ملف التطوير في `build/libs/` وتوجد النسخ الموزعة في `jar/`.

- [Minecraft 26.3 — JAR](../../jar/BetterVanilla+-1.1.0-26.3.jar)
- [Minecraft 26.4 Snapshot 7 — JAR](../../jar/BetterVanilla+-1.1.0-26.4-snapshot-7.jar)

---

## المحتويات

- [الرئيسية](ar-Home.md)
- [التثبيت](ar-Installation.md)
- [المصاعد](ar-Elevators.md)
- [وصفات التصنيع](ar-Recipes.md)
- [المراجل](ar-Cauldrons.md)
- [مكافأة المنارة](ar-Beacon.md)
- [الأوامر](ar-Commands.md)
- [الإصدار 1.1.0](ar-Release-1.1.0.md)
- [التوافق](ar-Compatibility.md)
- [الأسئلة الشائعة](ar-FAQ.md)

[المستودع](https://github.com/GoldenRose01/BetterVanilla-) | [الإبلاغ عن مشكلة](https://github.com/GoldenRose01/BetterVanilla-/issues)

</div>
