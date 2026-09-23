# Pipeline Canvas & Scoped Field Registry Pattern

Tài liệu này ghi nhận kiến trúc chuẩn hóa cho hệ thống đồ thị Pipeline Canvas và Dynamic Form Inspector trên Frontend.

---

## 1. Vấn đề cốt lõi & Giải pháp "Sàn" (Scoped Registry)

### Vấn đề trước refactor:
- `NodeConfigInspector.tsx` dài hàng trăm dòng, chứa đầy nhánh `if/else` thủ công (`isBoolean`, `isNumber`, `isAsset`, `isVariable`, `entityType`).
- **Prop Drilling sâu 5-6 tầng**: `PipelineCanvas` phải truyền `variables`, `projectId`, `edges`, `nodes` qua Inspector xuống `PinPropertyControl` rồi tới `EntityPinSelect`.
- **Rò rỉ domain**: Nếu đưa các controls đặc thù của Pipeline (VariableSelect, EntitySelect, AssetUpload, PathInput) vào `GlobalFieldRegistry` chung, chúng sẽ xuất hiện trong FormBuilder của Content Types.

### Giải pháp: Kiến trúc 2 Nhiệm vụ
1. **Cô lập Domain (Scoped Whitelisting):** `pipelineRegistry` là một instance của `ScopedFieldRegistry` kế thừa từ `baseRegistry`. Controls của Pipeline chỉ đăng ký trên sàn này, không ảnh hưởng tới Content hay Global Registry.
2. **Cung cấp Context theo Sàn (Scope Context):** `PipelineFormScopeProvider` inject context nghiệp vụ (`pipelineId`, `projectId`, `variables`, `edges`, `nodes`) tại Canvas. Các Form Controls bên dưới đọc qua hook `usePipelineFormScope()` mà không cần truyền prop.

```
┌─────────────────────────────────────────────────────────────┐
│               BASE REGISTRY (src/lib/field-registry.ts)     │
│   input | number | switch | select | textarea | colorPicker │
│   datePicker | file-upload | keyValue | tagsInput ...       │
└──────────────┬──────────────────────┬────────────────────────┘
               │                      │
               ▼                      ▼
┌──────────────────────────┐  ┌──────────────────────────────┐
│   PIPELINE REGISTRY      │  │   CONTENT REGISTRY           │
│   (pipelineRegistry)     │  │   (contentFieldRegistry)     │
│                          │  │                              │
│ + pin:variableSelect     │  │ + richText                   │
│ + pin:entitySelect       │  │ + slugInput                  │
│ + pin:assetUpload        │  │ + mediaLibrary               │
│ + pin:path               │  │                              │
│                          │  │ Provider: ContentFormScope   │
│ Provider:                │  │ Context: { contentTypeId,    │
│   PipelineFormScope      │  │   projectId }                │
│ Context: { pipelineId,   │  └──────────────────────────────┘
│   projectId, variables,  │
│   edges, nodes }         │
└──────────────────────────┘
```

---

## 2. Các thành phần chính

### 2.1. Scoped Field Registry (`src/lib/field-registry.ts`)
- Class `ScopedFieldRegistry` quản lý danh sách controls riêng và có liên kết `parent` (Scope Chain Lookup). Khi tìm kiếm một field type (`get(type)`), nếu không thấy ở sàn con thì sẽ tự động leo lên sàn cha.
- `baseRegistry` kế thừa `ScopedFieldRegistry` và liên kết với các controls toàn cục được nạp qua `import.meta.glob('../components/form-controls/Form*.tsx')`.

### 2.2. Scope Context (`src/features/pipelines/form-scope/PipelineFormScope.tsx`)
- Cung cấp:
  - `PipelineFormScopeProvider`: Bọc layout Canvas & Inspector.
  - `usePipelineFormScope()`: Hook lấy `pipelineId`, `projectId`, `variables`, `edges`, `nodes`.

### 2.3. Rule-based Strategy Factory (`src/features/pipelines/form-scope/pinToFieldDefinition.ts`)
Thay vì dùng chuỗi `if/else`, sử dụng Strategy Factory tuân thủ Open-Closed Principle (OCP):

```typescript
export interface PinFieldRule {
  predicate: (pin: PinDefinition, normType: NormalizedPinType, idLower: string) => boolean;
  create: (pin: PinDefinition, normType: NormalizedPinType, configValues: Record<string, any>) => FieldDefinition<any>;
}

export const PIN_RULES: PinFieldRule[] = [
  // 1. Variable Reference pin -> pin:variableSelect
  // 2. EntityRef pin -> pin:entitySelect
  // 3. Asset / Preset pin -> pin:assetUpload
  // 4. Boolean pin -> switch
  // 5. Number pin -> number
  // 6. Array cardinality -> tagsInput
  // 7. Map cardinality -> keyValue
  // 8. Script/Code pin -> textarea
  // 9. Path pin -> pin:path
];
```
Hàm `pinToFieldDefinition(pin, configValues)` là một **pure function** tìm rule đầu tiên thỏa mãn `predicate` để sinh ra `FieldDefinition` tương ứng.

### 2.4. NodeConfigInspector (`NodeConfigInspector.tsx`)
- Phân tách rõ rệt:
  - **Wired Inputs:** Chân cắm đã được nối dây từ node khác $\rightarrow$ hiển thị card thông tin kết nối (`Connected from <SourceNode> (<sourcePin>)`).
  - **Configurable Fields:** Chân cắm chưa nối dây $\rightarrow$ render bằng `<FormRenderer registry={pipelineRegistry} control={form.control} fields={formFields} />`.
- Quản lý state bằng `useForm` với cơ chế **Debounced Autosave (250ms)**: Khi người dùng gõ phím hoặc thay đổi giá trị, hệ thống tự động gửi mutation PATCH về backend mà không gây lag hay re-render toàn bộ canvas.

---

## 3. Quy trình thêm một Pin Type hoặc Form Control mới cho Pipeline

1. **Tạo Form Control:** Tạo file `src/features/pipelines/form-controls/FormPinXxx.tsx`.
   - Bọc qua `BaseFormField`.
   - Nếu cần context dự án/biến, gọi `usePipelineFormScope()`.
   - Cuối file, tự đăng ký vào sàn:
     ```typescript
     pipelineRegistry.register({
       type: "pin:xxx",
       component: FormPinXxx,
     });
     ```
2. **Khai báo nạp module:** Import file vừa tạo vào `src/features/pipelines/form-scope/pipelineRegistry.ts`.
3. **Thêm Rule Adapter:** Thêm một rule tương ứng vào mảng `PIN_RULES` trong `src/features/pipelines/form-scope/pinToFieldDefinition.ts`.

Không cần chỉnh sửa bất kỳ dòng code nào trong `NodeConfigInspector.tsx` hay `PipelineCanvas.tsx`.
