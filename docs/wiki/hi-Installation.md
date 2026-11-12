# स्थापना

भाषाएँ: [Italiano](Installazione.md) | [English](en-Installation.md) | [Español](es-Installation.md) | [简体中文](zh-Installation.md) | [हिन्दी](hi-Installation.md) | [العربية](ar-Installation.md) | [Português](pt-Installation.md) | [বাংলা](bn-Installation.md) | [Русский](ru-Installation.md) | [اردو](ur-Installation.md)

## आवश्यकताएँ

- चुने हुए JAR के अनुसार Minecraft Java **26.3** या **26.4 Snapshot 7**।
- गेम संस्करण के अनुकूल Fabric Loader।
- उसी Minecraft संस्करण का Fabric API।
- Java 25 या नया।

## स्थापना

1. चुने हुए Minecraft संस्करण के लिए Fabric Loader स्थापित करें।
2. अनुकूल Fabric API को `mods` में जोड़ें।
3. BetterVanilla+ 1.1.0 का **केवल एक** JAR `mods` में कॉपी करें।
4. संबंधित Fabric प्रोफ़ाइल से Minecraft चलाएँ।

नीचे दिए डाउनलोड में से एक चुनें; दोनों को एक ही प्रोफ़ाइल में स्थापित न करें।

## सर्वर और क्लाइंट

लिफ्ट इस्तेमाल करने और कस्टम आइटम देखने के लिए मॉड सर्वर और हर क्लाइंट पर होना चाहिए। एकल खिलाड़ी का एकीकृत सर्वर स्थानीय प्रोफ़ाइल की स्थापना ही इस्तेमाल करता है। इन्हीं परिवेशों में Fabric API भी होना चाहिए।

कमांड और ग्रामीणों के बदलाव सर्वर पर चलते हैं। लिफ्ट गतिविधि के कण क्लाइंट प्रभाव हैं और मॉड स्थापित करने वाले खिलाड़ियों को दिखाई देते हैं।

## प्रोजेक्ट बिल्ड

JDK 25 के साथ Windows पर `gradlew.bat build` चलाएँ। विकास JAR `build/libs/` में बनता है; वितरित बिल्ड `jar/` में हैं।

- [Minecraft 26.3 — JAR](../../jar/BetterVanilla+-1.1.0-26.3.jar)
- [Minecraft 26.4 Snapshot 7 — JAR](../../jar/BetterVanilla+-1.1.0-26.4-snapshot-7.jar)

---

## विषय सूची

- [मुखपृष्ठ](hi-Home.md)
- [स्थापना](hi-Installation.md)
- [लिफ्ट](hi-Elevators.md)
- [निर्माण विधियाँ](hi-Recipes.md)
- [कड़ाहे](hi-Cauldrons.md)
- [बीकन बोनस](hi-Beacon.md)
- [कमांड](hi-Commands.md)
- [संस्करण 1.1.0](hi-Release-1.1.0.md)
- [अनुकूलता](hi-Compatibility.md)
- [अक्सर पूछे जाने वाले प्रश्न](hi-FAQ.md)

[रिपॉज़िटरी](https://github.com/GoldenRose01/BetterVanilla-) | [समस्या बताएँ](https://github.com/GoldenRose01/BetterVanilla-/issues)
