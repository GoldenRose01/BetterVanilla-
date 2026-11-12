# कमांड

भाषाएँ: [Italiano](Comandi.md) | [English](en-Commands.md) | [Español](es-Commands.md) | [简体中文](zh-Commands.md) | [हिन्दी](hi-Commands.md) | [العربية](ar-Commands.md) | [Português](pt-Commands.md) | [বাংলা](bn-Commands.md) | [Русский](ru-Commands.md) | [اردو](ur-Commands.md)

`/gameset` कमांड के लिए Minecraft की प्रशासनिक अनुमति `COMMANDS_GAMEMASTER` चाहिए। KeepInventory का लक्षित खिलाड़ी ऑनलाइन होना चाहिए।

## व्यक्तिगत KeepInventory

```text
/gameset player keepinventory on <player>
/gameset player keepinventory off <player>
```

निर्दिष्ट खिलाड़ी की मृत्यु पर इन्वेंटरी और अनुभव बचाना चालू या बंद करता है। यह व्यक्तिगत सेटिंग वैश्विक `keepInventory` गेम नियम से स्वतंत्र है और खिलाड़ी के डेटा में सहेजी जाती है।

## ग्रामीण व्यापार संपादक

```text
/gameset villagertrade
```

क्षैतिज दिशा में लगभग 16 ब्लॉक के भीतर सबसे पास के ग्रामीण का संपादक खोलता है। प्रस्ताव बना, बदल या हटा सकते हैं और निर्धारित कर सकते हैं:

- मुख्य लागत का आइटम और मात्रा।
- वैकल्पिक दूसरी लागत का आइटम और मात्रा।
- मिलने वाला आइटम और मात्रा।
- अधिकतम उपयोगों की संख्या।

इन्वेंटरी में आइटम उठाकर संपादक के संबंधित स्लॉट पर क्लिक करें। मेनू उसका प्रकार कॉपी करता है, पकड़ा हुआ आइटम खर्च नहीं करता। ग्रामीण न मिलने पर त्रुटि दिखाई जाती है।

सहेजे बदलाव ग्रामीण पर लागू होते हैं और दुनिया में बने रहते हैं। मेनू खुला रहने तक ग्रामीण के पास रहें।

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
