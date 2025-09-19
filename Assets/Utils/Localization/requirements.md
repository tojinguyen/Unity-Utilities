# Localization System Requirements

## Tổng quan
Hệ thống đa ngôn ngữ (Localization) cho Unity với đầy đủ tính năng quản lý, chỉnh sửa và sử dụng các ngôn ngữ khác nhau trong game. Hỗ trợ cả Editor Tools và Runtime components.

---

## 🛠️ Tính năng Editor Tool

### 1. Quản lý ngôn ngữ
- **Tạo/xoá/sửa danh sách ngôn ngữ**: Hỗ trợ thêm các ngôn ngữ như English, Vietnamese, Japanese, Chinese, Arabic, Hebrew, v.v.
- **Chọn ngôn ngữ mặc định**: Thiết lập ngôn ngữ mặc định khi không có dữ liệu hoặc ngôn ngữ người dùng không được hỗ trợ
- **Metadata ngôn ngữ**: Lưu trữ thông tin như:
  - Tên hiển thị (Display Name)
  - Mã ngôn ngữ (Language Code: en, vi, ja...)
  - Hướng văn bản (Text Direction: LTR/RTL)
  - Font mặc định cho ngôn ngữ

### 2. Quản lý key
- **Thêm/xoá/sửa key**: Quản lý các key localization (ví dụ: `MAIN_MENU_PLAY`, `UI_BUTTON_CONFIRM`)
- **Hiển thị bảng dịch**: Interface dạng bảng (Excel-like) với:
  - Cột đầu: Key name
  - Các cột tiếp theo: Từng ngôn ngữ
  - Cho phép edit trực tiếp trong bảng
- **Tìm kiếm key**: 
  - Tìm theo tên key
  - Tìm theo nội dung translation
  - Filter theo ngôn ngữ hoặc trạng thái translation

### 3. Import/Export
- **Format hỗ trợ**: CSV và JSON
- **Import CSV/JSON**: 
  - Mapping cột ngôn ngữ tự động
  - Validate format và cấu trúc
  - Preview trước khi import
- **Export CSV/JSON**: 
  - Export toàn bộ hoặc theo ngôn ngữ cụ thể
  - Template CSV cho translator
  - Backup và versioning

### 4. Preview trong Editor
- **Language Selector**: Dropdown chọn ngôn ngữ trong:
  - Editor Toolbar
  - Custom EditorWindow
  - Scene View controls
- **Real-time Preview**: 
  - Khi đổi ngôn ngữ → update text trong Scene ngay lập tức
  - UI Text Binder auto refresh
  - Preview cả font changes và layout

### 5. Validation & Debug
- **Missing Translation Detection**: 
  - Báo lỗi key thiếu translation
  - Highlight các key chưa hoàn thành
  - Report theo ngôn ngữ
- **Duplicate Key Detection**: 
  - Phát hiện key trùng lặp
  - Warning khi tạo key đã tồn tại
- **Project Scan**: 
  - Scan toàn project tìm prefab/scene sử dụng key không tồn tại
  - Dead key detection (key không được sử dụng)
  - Reference tracking

---

## 🎮 Tính năng Runtime

### 1. Localization Manager
- **Singleton Pattern**: Quản lý trung tâm cho hệ thống localization
- **Core API**:
  - `Localize(key)`: Lấy text theo key
  - `SetLanguage(languageCode)`: Đổi ngôn ngữ runtime
  - `GetCurrentLanguage()`: Lấy ngôn ngữ hiện tại
  - `GetAvailableLanguages()`: Danh sách ngôn ngữ hỗ trợ
- **Persistence**: Lưu ngôn ngữ người chơi đã chọn (PlayerPrefs/SaveData)
- **Events**: Callback khi đổi ngôn ngữ để các component update

### 2. UI Text Binder
- **Component Support**: 
  - Text (Legacy)
  - TextMeshPro (TMP_Text)
  - UI Button text
  - Custom text components
- **Features**:
  - Field `key` để bind với Localization Manager
  - Auto update khi đổi ngôn ngữ runtime
  - Format string support (với parameters)
  - Fallback text khi key không tồn tại

### 3. Auto Language Detection
- **System Language**: Tự động lấy system language khi start game
- **Fallback Logic**: 
  - Nếu system language không được support → fallback về ngôn ngữ mặc định
  - User preference override system detection
- **First Launch**: Logic xử lý lần đầu chạy game

### 4. Font & Layout Handling
- **Multi-font Support**: 
  - Font khác nhau cho các ngôn ngữ (Latin, Japanese, Chinese, Arabic)
  - Automatic font switching khi đổi ngôn ngữ
- **Text Direction**: 
  - Left-to-Right (LTR) cho hầu hết ngôn ngữ
  - Right-to-Left (RTL) cho Arabic, Hebrew
- **Layout Adjustment**: 
  - Text alignment tự động
  - UI layout resize theo content length

### 5. Asset Localization (Optional)
- **Asset Types**: 
  - Images/Sprites (flag icons, UI graphics)
  - Audio clips (voice-over, sound effects)
  - Video clips (tutorials, cutscenes)
- **Asset Management**: 
  - Folder structure theo ngôn ngữ
  - Automatic asset loading theo current language
  - Fallback assets khi không tìm thấy

---

## 🔮 Tính năng Nâng cao (Optional)

### 1. Context-based Translation
- **Variant Support**: Cùng 1 key nhưng nhiều variant
  - Số ít/số nhiều (singular/plural)
  - Giới tính nhân vật (gender-based)
  - Context khác nhau (formal/informal)
- **Conditional Logic**: Rules để chọn variant phù hợp

### 2. Plurals & Formatting
- **Number Formatting**: 
  - "1 item" vs "5 items"
  - Currency formatting theo locale
  - Date/time formatting
- **String Interpolation**: 
  - Parameter substitution: `"Hello {playerName}"`
  - Rich text support: `"<color=red>Warning</color>"`

### 3. Hot Reload
- **External File Sync**: 
  - Monitor CSV/JSON files changes
  - Auto reload without rebuild
  - Development mode feature
- **Live Update**: Update translations trong runtime không cần restart

### 4. Cloud Integration
- **Google Sheets API**: 
  - Sync trực tiếp với Google Sheets
  - Collaborative translation workflow
  - Version control cho translations
- **Translation Services**: 
  - Integration với Google Translate API
  - Auto-translation suggestions
  - Quality assurance tools

---

## 📋 Technical Requirements

### Editor Requirements
- Unity 2020.3 LTS trở lên
- Custom EditorWindow cho localization management
- ScriptableObject cho data storage
- Custom PropertyDrawer cho UI components

### Runtime Requirements
- Lightweight, performance-optimized
- Memory-efficient string storage
- Fast key lookup (Dictionary-based)
- Thread-safe operations

### Data Format
- **Primary**: ScriptableObject assets
- **Export**: CSV, JSON
- **Backup**: Version control friendly format

### Integration
- Package Manager compatible
- No external dependencies (core features)
- Optional dependencies cho advanced features

---

## 🎯 Deliverables

### Phase 1 - Core System
1. Localization Manager (Runtime)
2. Basic Editor Tool (key management)
3. UI Text Binder component
4. CSV Import/Export

### Phase 2 - Enhanced Editor
1. Advanced Editor UI (table view)
2. Validation & debugging tools
3. Preview system
4. Project scanning

### Phase 3 - Advanced Features
1. Font & layout handling
2. Asset localization
3. Context-based translations
4. Hot reload system

### Phase 4 - Cloud & Integration
1. Google Sheets integration
2. Translation service integration
3. Advanced formatting
4. Performance optimizations

---

## 🧪 Testing Requirements

### Unit Tests
- Localization Manager API
- Key lookup performance
- Data validation logic

### Integration Tests
- Editor tool functionality
- Import/Export accuracy
- UI component binding

### Performance Tests
- Large dataset handling (10,000+ keys)
- Memory usage optimization
- Loading time benchmarks

---

## 📚 Documentation Requirements

### User Documentation
- Setup guide
- Editor tool tutorials
- Runtime API reference
- Best practices guide

### Developer Documentation
- Architecture overview
- Extension points
- Custom component creation
- Troubleshooting guide