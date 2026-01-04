# API Test - Hệ thống Quản lý Điện Nước

##  1. Xác thực (Authentication)

### 1.1. Đăng nhập (Login)
**POST** `http://localhost:5058/api/auth/login`
```json

{
  "username": "admin",
  "password": "Admin@123"
}
```
> **Tip**: Copy chuỗi `token` trả về và dán vào tab **Auth** -> **Bearer Token** cho các yêu cầu bên dưới.

### 1.2. Đăng ký & Tự động tạo Khách hàng (Register)
**POST** `http://localhost:5058/api/auth/register`
*Quyền: Admin*
```json
{
  "username": "customer01",
  "password": "Password@123",
  "email": "customer01@example.com",
  "fullName": "Nguyễn Văn Khách",
  "role": 3, // 1=Admin, 2=Staff, 3=Customer
  "address": "123 Đường Láng, Hà Nội",
  "phoneNumber": "0912345678",
  "identityCard": "001234567890"
}
```

---

##  2. Khách hàng (Customers)

### 2.1. Danh sách khách hàng
**GET** `http://localhost:5058/api/customers`

### 2.2. Chi tiết khách hàng
**GET** `http://localhost:5058/api/customers/1`

### 2.3. Cập nhật khách hàng
**PUT** `http://localhost:5058/api/customers/1`
*Quyền: Admin, Staff*
```json
{
  "fullName": "Nguyễn Văn Khách Update",
  "address": "Số 10 Trần Duy Hưng",
  "phoneNumber": "0988888888",
  "email": "updated@gmail.com",
  "isActive": true
}
```

### 2.4. Xóa khách hàng
**DELETE** `http://localhost:5058/api/customers/1`
*Quyền: Admin, Staff*

---

##  3. Công tơ (Meters)

### 3.1. Danh sách công tơ
**GET** `http://localhost:5058/api/meters`

### 3.2. Công tơ theo khách hàng
**GET** `http://localhost:5058/api/meters/customer/1`

### 3.3. Thêm công tơ mới
**POST** `http://localhost:5058/api/meters`
```json
{
  "meterNumber": "DH-001234",
  "type": 1,
  "installDate": "2026-01-02T00:00:00",
  "location": "Tầng 1, Phòng 101",
  "initialReading": 0,
  "customerId": 1
}
```

---

##  4. Chỉ số (Readings)

### 4.1. Danh sách tất cả chỉ số
**GET** `http://localhost:5058/api/readings`

### 4.2. Chi tiết chỉ số
**GET** `http://localhost:5058/api/readings/1`

### 4.3. Chỉ số theo công tơ
**GET** `http://localhost:5058/api/readings/meter/1`

### 4.4. Nhập chỉ số mới
**POST** `http://localhost:5058/api/readings`
*Quyền: Admin, Staff*
```json
{
  "meterId": 1,
  "readingDate": "2024-12-31T08:00:00",
  "currentReading": 1500.5,
  "notes": "Chốt số cuối tháng"
}
```

### 4.5. Cập nhật chỉ số
**PUT** `http://localhost:5058/api/readings/1`
*Quyền: Admin, Staff*
>  **Lưu ý:** Không thể sửa chỉ số đã tạo hóa đơn!
```json
{
  "readingDate": "2024-12-31T10:00:00",
  "currentReading": 1550,
  "notes": "Đã sửa lại chỉ số"
}
```

### 4.6. Xóa chỉ số
**DELETE** `http://localhost:5058/api/readings/1`
*Quyền: Admin, Staff*
>  **Lưu ý:** Không thể xóa chỉ số đã tạo hóa đơn!

---

##  5. Hóa đơn & Thanh toán (Bills & Payments)

### 5.1. Tạo hóa đơn tự động
**POST** `http://localhost:5058/api/bills/generate`
```json
{
  "readingId": 1,
  "dueDate": "2025-01-15T23:59:59"
}
```

### 5.2. Thanh toán hóa đơn
**POST** `http://localhost:5058/api/payments`
```json
{
  "billId": 1,
  "amount": 500000,
  "method": 2, // 1=Tiền mặt, 2=Chuyển khoản
  "transactionId": "TXN_123456",
  "notes": "Thanh toán qua ngân hàng"
}
```

### 5.3. Tải PDF hóa đơn
**GET** `http://localhost:5058/api/bills/1/pdf`

---

##  6. Báo cáo (Reports)

### 6.1. Báo cáo doanh thu
**GET** `http://localhost:5058/api/reports/revenue?startDate=2024-12-01&endDate=2024-12-31`

### 6.2. Hóa đơn nợ quá hạn
**GET** `http://localhost:5058/api/reports/outstanding`

---

##  7. Quản lý Nợ (Debt Management)
*Quyền: Admin*

### 7.1. Danh sách khách hàng nợ
**GET** `http://localhost:5058/api/debt-management/customers`

### 7.2. Cắt dịch vụ do nợ
**POST** `http://localhost:5058/api/debt-management/suspend/{customerId}`

Ví dụ: `http://localhost:5058/api/debt-management/suspend/1`
```json
{
  "notes": "Nợ tiền 3 tháng"
}
```

### 7.3. Khôi phục dịch vụ
**POST** `http://localhost:5058/api/debt-management/restore/{customerId}`

Ví dụ: `http://localhost:5058/api/debt-management/restore/1`
```json
{
  "notes": "Đã thanh toán xong"
}
```
