---
name: create_feature
description: HÆ°á»›ng dáº«n cÃ¡ch táº¡o má»™t tÃ­nh nÄƒng (Feature/Vertical Slice) má»›i trong má»™t module cá»§a dá»± Ã¡n.
---

# HÆ°á»›ng Dáº«n Táº¡o TÃ­nh NÄƒng Má»›i (Feature)

Dá»± Ã¡n Ã¡p dá»¥ng kiáº¿n trÃºc Vertical Slice (VSA), do Ä‘Ã³ má»—i tÃ­nh nÄƒng sáº½ Ä‘Æ°á»£c Ä‘Ã³ng gÃ³i trong má»™t thÆ° má»¥c riÃªng biá»‡t táº¡i `Features/<FeatureGroup>/<FeatureName>`. 

### TrÆ°á»ng há»£p 1: TÃ­nh nÄƒng lÃ  nhÃ³m CRUD tiÃªu chuáº©n
Náº¿u tÃ­nh nÄƒng báº¡n cáº§n lÃ m bao gá»“m cÃ¡c thao tÃ¡c Create, Update, Delete, GetById, GetList (CRUD) tiÃªu chuáº©n cho má»™t Entity, Báº®T BUá»˜C sá»­ dá»¥ng cÃ´ng cá»¥ CLI:

```bash
dotnet run --project tools/Automation.Cli -- add-crud <TÃªnModule> <TÃªnEntity>
```
*VÃ­ dá»¥:* `dotnet run --project tools/Automation.Cli -- add-crud Billing Invoice`
Lá»‡nh nÃ y sáº½ tá»± Ä‘á»™ng sinh ra Ä‘áº§y Ä‘á»§ bá»™ Entity, Dto, Group Endpoint, vÃ  cÃ¡c thÆ° má»¥c Slice tÆ°Æ¡ng á»©ng.

### TrÆ°á»ng há»£p 2: TÃ­nh nÄƒng Ä‘Æ¡n láº» (Custom Slice)
Náº¿u báº¡n chá»‰ cáº§n táº¡o má»™t tÃ­nh nÄƒng Ä‘Æ¡n láº» (khÃ´ng pháº£i toÃ n bá»™ bá»™ CRUD), hÃ£y táº¡o thÆ° má»¥c má»›i thá»§ cÃ´ng hoáº·c sao chÃ©p tá»« má»™t tÃ­nh nÄƒng cÃ³ sáºµn. Má»™t tÃ­nh nÄƒng (Slice) hoÃ n chá»‰nh thÆ°á»ng bao gá»“m:

1. **`{ActionName}Command.cs`** hoáº·c **`{ActionName}Query.cs`**:
   Lá»›p chá»©a dá»¯ liá»‡u Ä‘áº§u vÃ o.
2. **`{ActionName}Validator.cs`**:
   Lá»›p káº¿ thá»«a `Validator<T>` (tá»« FluentValidation) Ä‘á»ƒ kiá»ƒm tra tÃ­nh há»£p lá»‡ cá»§a Request.
3. **`{ActionName}Handler.cs`**:
   Lá»›p xá»­ lÃ½ nghiá»‡p vá»¥. **Báº¯t buá»™c sá»­ dá»¥ng class thÆ°á»ng vá»›i constructor injection** (VD: tiÃªm `DbContext`), khÃ´ng sá»­ dá»¥ng static class/method.
4. **`{ActionName}Endpoint.cs`**:
   Káº¿ thá»«a tá»« `Endpoint<TRequest, TResponse>` cá»§a FastEndpoints. Nhá»› gá»i phÆ°Æ¡ng thá»©c `Group<TGroup>()` trá» vá» Endpoint Group tÆ°Æ¡ng á»©ng cá»§a Feature.

**LÆ°u Ã½:** Sau khi táº¡o, hÃ£y cháº¡y `dotnet build` Ä‘á»ƒ kiá»ƒm tra Wolverine tá»± Ä‘á»™ng gáº¯n káº¿t Handler vÃ  FastEndpoints tá»± Ä‘á»™ng phÃ¡t hiá»‡n API.


