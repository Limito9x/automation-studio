# Agent Context — HybridVSA Frontend

Đọc file này trước mọi task. Đây là summary nhanh để không cần đọc toàn bộ ARCHITECTURE.md.

## Stack nhanh
React 19, TypeScript, Vite, Tailwind v4, shadcn/ui (Nova), TanStack Router (file-based),
TanStack Query v5, React Hook Form, Zod, hey-api, Temporal API + temporal-polyfill, Lucide

## Cấu trúc quan trọng
```
src/api/generated/      ← KHÔNG SỬA, chỉ re-generate
src/components/ui/      ← KHÔNG SỬA, shadcn primitives
src/lib/api-client.ts   ← error handling tập trung tại đây
src/features/[name]/    ← mỗi feature: hooks/ + components/ + types.ts + index.ts
src/routes/             ← TanStack Router file-based
```

## Rules tóm tắt
- Màu/spacing → dùng shadcn token, KHÔNG hard-code
- API → generated function → hook (useQuery/useMutation) → component, không skip bước
- Filter/pagination → URL search params, KHÔNG useState
- Form → RHF + Zod, KHÔNG validate thủ công
- Date → Temporal khắp nơi, Date object chỉ tại boundary shadcn Calendar
- Error handling → api-client.ts middleware, KHÔNG try-catch trong component

## Khi tạo feature mới — thứ tự file
types.ts → hooks → components → index.ts → route

## Tham khảo đầy đủ
- ARCHITECTURE.md — cấu trúc, data flow, layer responsibilities
- FRONTEND-RULES.md — rules chi tiết kèm ví dụ code