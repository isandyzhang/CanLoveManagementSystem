# NGO 個案管理系統 - 資料庫結構設計

## 資料庫架構概覽

本系統採用關聯式資料庫設計，以個案為核心，延伸出多個評估子表，形成完整的個案管理體系。

### 主要資料表關係

#### 📋 核心個案表
- **Cases** (個案基本資料) - 主表

#### 📝 個案詳細資料
- **CaseDetail** (個案詳細資料) - 1:1 關聯
- **CaseSocialWorkerContent** (社會工作服務內容) - 1:1 關聯  
- **CaseFQeconomicStatus** (經濟狀況評估) - 1:1 關聯
- **CaseHQhealthStatus** (健康狀況評估) - 1:1 關聯
- **CaseIQacademicPerformance** (學業表現評估) - 1:1 關聯
- **CaseEQemotionalEvaluation** (情緒評估) - 1:1 關聯
- **FinalAssessmentSummary** (最後評估表) - 1:1 關聯

#### 👥 個案關聯子表
- **CaseFamilySpecialStatus** (家庭特殊身份) - 1:* 關聯
- **CaseFamilyMembers** (個案家中成員) - 1:* 關聯
- **CaseFamilyMemberNotes** (家庭成員備註) - 1:* 關聯
- **CaseConsultationRecords** (個案會談輔導紀錄) - 1:* 關聯
- **CaseSocialWorkerServices** (社會工作服務內容) - 1:* 關聯

#### 🏫 選項參考表
- **Schools** (學校選項) - 參考表
- **Cities** (城市選項) - 參考表
- **Districts** (地區選項) - 參考表
- **Nationalities** (國籍選項) - 參考表
- **FamilyStructureTypes** (家庭結構類型) - 參考表
- **OptionSets / OptionSetValues** (通用選項系統) - 參考表

#### 📚 歷史記錄表
- **CaseHistory** (個案歷史記錄) - 1:* 關聯
- **CaseDetailHistory** (個案詳細資料歷史記錄) - 1:* 關聯

#### 🔍 稽核與活動記錄
- **DataChangeLog** (資料變更紀錄) - 稽核表
- **UserActivityLog** (使用者活動紀錄) - 稽核表

---

## 📋 核心個案表

### 1. Cases - 個案基本資料表

| 欄位名稱 | 中文名稱 | 資料型態 | 說明 | 約束條件 |
|:---------|:---------|:----------|:-----|:---------|
| **caseID** | **個案編號** | **NVARCHAR(10)** | **主鍵，手動輸入** | **PRIMARY KEY, NOT NULL** |
| **assessment_date** | **評估日期** | **DATE** | **個案評估的日期** | **NULL** |
| name | 姓名 | NVARCHAR(20) | 必填 | NOT NULL |
| gender | 性別 | NCHAR(2) | 男/女 | CHECK (gender IN (N'男', N'女')) |
| school_id | 就讀學校 | INT | 外鍵，關聯 Schools.school_id | FOREIGN KEY, NULL |
| birth_date | 出生日 | DATE |  | NOT NULL |
| id_number | 身分證字號(加密) | NVARCHAR(255) | 加密後的身分證字號 | UNIQUE, NOT NULL |
| address | 詳細地址 | NVARCHAR(25) |  |  |
| city_id | 城市 | INT | 外鍵，關聯 Cities.city_id | FOREIGN KEY, NULL |
| district_id | 地區/鄉鎮區 | INT | 外鍵，關聯 Districts.district_id | FOREIGN KEY, NULL |
| phone | 連絡電話 | NVARCHAR(15) |  |  |
| email | 電子郵箱 | NVARCHAR(50) |  |  |
| photo | 照片/圖檔路徑 | NVARCHAR(100) |  |  |
| **draft_status** | **草稿狀態** | **BIT** | **0=草稿, 1=完成** | **DEFAULT 0** |
| **submitted_by** | **提交者** | **NVARCHAR(30)** | **A 人員的 ID** | **NULL** |
| **submitted_at** | **提交時間** | **DATETIME2** | **A 提交檢核的時間** | **NULL** |
| **reviewed_by** | **檢核者** | **NVARCHAR(30)** | **B 主管的 ID** | **NULL** |
| **reviewed_at** | **檢核時間** | **DATETIME2** | **B 主管檢核的時間** | **NULL** |
| deleted | 軟刪除標記 | BIT | 0=正常, 1=已刪除 | DEFAULT 0 |
| deleted_at | 刪除時間 | DATETIME2 | 軟刪除時間戳記 | NULL |
| deleted_by | 刪除者 | NVARCHAR(30) | 執行刪除的使用者 | NULL |
| **is_locked** | **個案鎖定狀態** | **BIT** | **0=未鎖定, 1=已鎖定** | **DEFAULT 0** |
| **locked_by** | **鎖定者** | **NVARCHAR(30)** | **鎖定個案的使用者ID** | **NULL** |
| **locked_at** | **鎖定時間** | **DATETIME2** | **個案被鎖定的時間** | **NULL** |
| created_at | 建立時間 | DATETIME2 | 系統自動 | DEFAULT GETDATE() |
| updated_at | 更新時間 | DATETIME2 | 系統自動 | DEFAULT GETDATE() |

---

## 📝 個案詳細資料

### 2. CaseDetail - 個案詳細資料表

| 欄位名稱 | 中文名稱 | 資料型態 | 說明 | 約束條件 |
|:---------|:---------|:----------|:-----|:---------|
| caseID | 個案編號 | NVARCHAR(10) | 外鍵，關聯 Cases.caseID | FOREIGN KEY, NOT NULL |
| contact_name | 聯絡人姓名 | NVARCHAR(20) |  |  |
| contact_relation_value_id | 與案主關係 | INT | 外鍵，關聯 OptionSetValues.option_value_id（CONTACT_RELATION） | FOREIGN KEY, NULL |
| contact_phone | 聯絡人電話 | NVARCHAR(15) |  |  |
| home_phone | 住家電話 | NVARCHAR(15) |  |  |
| family_structure_type_id | 家庭結構類型 | INT | 外鍵，關聯 FamilyStructureTypes.structure_type_id | FOREIGN KEY, NULL |
| family_structure_other_desc | 其他結構描述 | NVARCHAR(100) | 當選擇「其他」時填寫 | NULL |
| parent_nation_father_id | 父親國籍 | INT | 外鍵，關聯 Nationalities.nationality_id | FOREIGN KEY, NULL |
| parent_nation_mother_id | 母親國籍 | INT | 外鍵，關聯 Nationalities.nationality_id | FOREIGN KEY, NULL |
| main_caregiver_name | 主要照顧者姓名 | NVARCHAR(20) |  |  |
| main_caregiver_relation | 主要照顧者與案主關係 | NVARCHAR(10) |  |  |
| main_caregiver_id | 主要照顧者身分證字號(加密) | NVARCHAR(255) | 加密後的身分證字號 |  |
| main_caregiver_birth | 主要照顧者生日 | DATE |  |  |
| main_caregiver_job | 主要照顧者職業 | NVARCHAR(30) |  |  |
| main_caregiver_marry_status_value_id | 主要照顧者婚姻狀況 | INT | 外鍵，關聯 OptionSetValues.option_value_id（MARITAL_STATUS） | FOREIGN KEY, NULL |
| main_caregiver_edu_value_id | 主要照顧者教育程度 | INT | 外鍵，關聯 OptionSetValues.option_value_id（EDUCATION_LEVEL） | FOREIGN KEY, NULL |
| source_value_id | 個案來源 | INT | 外鍵，關聯 OptionSetValues.option_value_id（CASE_SOURCE） | FOREIGN KEY, NULL |
| help_experience_value_id | 求助經驗 | INT | 外鍵，關聯 OptionSetValues.option_value_id（HELP_EXPERIENCE） | FOREIGN KEY, NULL |
| note | 備註 | NVARCHAR(1000) |  |  |
| deleted | 軟刪除標記 | BIT | 0=正常, 1=已刪除 | DEFAULT 0 |
| deleted_at | 刪除時間 | DATETIME2 | 軟刪除時間戳記 | NULL |
| deleted_by | 刪除者 | NVARCHAR(30) | 執行刪除的使用者 | NULL |
| created_at | 建立時間 | DATETIME2 | 系統自動 | DEFAULT GETDATE() |
| updated_at | 更新時間 | DATETIME2 | 系統自動 | DEFAULT GETDATE() |

---

### 3. CaseSocialWorkerContent - 社會工作服務內容表

| 欄位名稱 | 中文名稱 | 資料型態 | 說明 | 約束條件 |
|:---------|:---------|:----------|:-----|:---------|
| caseID | 個案編號 | NVARCHAR(10) | 外鍵，關聯 Cases.caseID | FOREIGN KEY, NOT NULL |
| family_tree_img | 家系圖圖片 | NVARCHAR(250) |  |  |
| residence_type_value_id | 居住地型態 | INT | 外鍵，關聯 OptionSetValues.option_value_id（RESIDENCE_TYPE） | FOREIGN KEY, NULL |
| house_cleanliness_rating | 居家整潔分數 | TINYINT | 1-4分評分 |  |
| house_cleanliness_note | 居家整潔說明 | NVARCHAR(50) |  |  |
| house_safety_rating | 居家安全分數 | TINYINT | 1-4分評分 |  |
| house_safety_note | 居家安全說明 | NVARCHAR(50) |  |  |
| caregiver_child_interaction_rating | 照顧者與兒少互動情形分數 | TINYINT | 1-4分評分 |  |
| caregiver_child_interaction_note | 照顧者與兒少互動情形說明 | NVARCHAR(50) |  |  |
| caregiver_family_interaction_rating | 照顧者與整體同住家人互動情形分數 | TINYINT | 1-4分評分 |  |
| caregiver_family_interaction_note | 照顧者與整體同住家人互動情形說明 | NVARCHAR(50) |  |  |
| family_resource_ability_rating | 家庭資源連結能力分數 | TINYINT | 1-4分評分 |  |
| family_resource_ability_note | 家庭資源連結能力說明 | NVARCHAR(50) |  |  |
| family_social_support_rating | 社會家庭支持獲得分數 | TINYINT | 1-4分評分 |  |
| family_social_support_note | 社會家庭支持獲得說明 | NVARCHAR(50) |  |  |
| special_circumstances_description | 其他特殊情形描述 | NVARCHAR(50) |  |  |
| deleted | 軟刪除標記 | BIT | 0=正常, 1=已刪除 | DEFAULT 0 |
| deleted_at | 刪除時間 | DATETIME2 | 軟刪除時間戳記 | NULL |
| deleted_by | 刪除者 | NVARCHAR(30) | 執行刪除的使用者 | NULL |
| created_at | 建立時間 | DATETIME2 | 系統自動 | DEFAULT GETDATE() |
| updated_at | 更新時間 | DATETIME2 | 系統自動 | DEFAULT GETDATE() |

---

### 4. CaseFQeconomicStatus - 經濟狀況評估表 (FQ)

| 欄位名稱 | 中文名稱 | 資料型態 | 說明 | 約束條件 |
|:---------|:---------|:----------|:-----|:---------|
| caseID | 個案編號 | NVARCHAR(10) | 外鍵，關聯 Cases.caseID | FOREIGN KEY, NOT NULL |
| economic_overview | 家庭經濟概況描述 | NVARCHAR(50) |  |  |
| work_situation | 工作情形 | NVARCHAR(50) |  |  |
| civil_welfare_resources | 民間福利資源 | NVARCHAR(50) |  |  |
| monthly_income | 月收入 | DECIMAL(10,2) | 家庭月收入 |  |
| monthly_expense | 月支出 | DECIMAL(10,2) | 家庭月支出 |  |
| monthly_expense_note | 月支出說明 | NVARCHAR(50) |  |  |
| description | 描述 | NVARCHAR(50) |  |  |
| deleted | 軟刪除標記 | BIT | 0=正常, 1=已刪除 | DEFAULT 0 |
| deleted_at | 刪除時間 | DATETIME2 | 軟刪除時間戳記 | NULL |
| deleted_by | 刪除者 | NVARCHAR(30) | 執行刪除的使用者 | NULL |
| created_at | 建立時間 | DATETIME2 | 系統自動 | DEFAULT GETDATE() |
| updated_at | 更新時間 | DATETIME2 | 系統自動 | DEFAULT GETDATE() |

---

### 5. CaseHQhealthStatus - 家庭身心概況 (HQ)

| 欄位名稱 | 中文名稱 | 資料型態 | 說明 | 約束條件 |
|:---------|:---------|:----------|:-----|:---------|
| caregiver_id | 照顧者編號 | INT | 主鍵，自動遞增 | PRIMARY KEY, IDENTITY(1,1) |
| caseID | 個案編號 | NVARCHAR(10) | 外鍵，關聯 Cases.caseID | FOREIGN KEY, NOT NULL |
| caregiver_role_value_id | 照顧者角色 | INT | 外鍵，關聯 OptionSetValues.option_value_id（CARE_GIVER_ROLE） | FOREIGN KEY, NOT NULL |
| caregiver_name | 照顧者姓名 | NVARCHAR(50) | 非家中成員時可填 | NULL |
| is_primary | 是否主要照顧者 | BIT | 1=主要,0=否 | DEFAULT 0 |
| **emotional_expression_rating** | **情緒表現分數** | **TINYINT** | **1-4分評分（照顧者用）** | **CHECK (emotional_expression_rating IN (1,2,3,4))** |
| **emotional_expression_note** | **情緒表現說明** | **NVARCHAR(50)** | **照顧者情緒表現說明** |  |
| **health_status_rating** | **身體健康分數** | **TINYINT** | **1-4分評分（照顧者用）** | **CHECK (health_status_rating IN (1,2,3,4))** |
| **health_status_note** | **身體健康說明** | **NVARCHAR(50)** | **照顧者身體健康說明** |  |
| **child_health_status_rating** | **兒少健康狀態分數** | **TINYINT** | **1-4分評分（被照顧者用）** | **CHECK (child_health_status_rating IN (1,2,3,4)), NOT NULL** |
| **child_health_status_note** | **兒少健康狀態說明** | **NVARCHAR(50)** | **被照顧者健康狀態說明** |  |
| **child_care_status_rating** | **兒少受照顧狀態分數** | **TINYINT** | **1-4分評分（被照顧者用）** | **CHECK (child_care_status_rating IN (1,2,3,4)), NOT NULL** |
| **child_care_status_note** | **兒少受照顧狀態說明** | **NVARCHAR(50)** | **被照顧者受照顧狀態說明** |  |
| deleted | 軟刪除標記 | BIT | 0=正常, 1=已刪除 | DEFAULT 0 |
| deleted_at | 刪除時間 | DATETIME2 | 軟刪除時間戳記 | NULL |
| deleted_by | 刪除者 | NVARCHAR(30) | 執行刪除的使用者 | NULL |
| created_at | 建立時間 | DATETIME2 | 系統自動 | DEFAULT GETDATE() |
| updated_at | 更新時間 | DATETIME2 | 系統自動 | DEFAULT GETDATE() |

#### 使用邏輯說明
- **當 caregiver_role_value_id = 被照顧者本身時**：使用 `child_health_status_rating` 和 `child_care_status_rating` 欄位
- **當 caregiver_role_value_id = 其他照顧者時**：使用 `emotional_expression_rating` 和 `health_status_rating` 欄位

---

### 6. CaseIQacademicPerformance - 學業表現評估表 (IQ)

| 欄位名稱 | 中文名稱 | 資料型態 | 說明 | 約束條件 |
|:---------|:---------|:----------|:-----|:---------|
| caseID | 個案編號 | NVARCHAR(10) | 外鍵，關聯 Cases.caseID | FOREIGN KEY, NOT NULL |
| **academic_performance_summary** | **學業表現描述** | **NVARCHAR(100)** | **學業表現綜合描述** | **NULL** |
| deleted | 軟刪除標記 | BIT | 0=正常, 1=已刪除 | DEFAULT 0 |
| deleted_at | 刪除時間 | DATETIME2 | 軟刪除時間戳記 | NULL |
| deleted_by | 刪除者 | NVARCHAR(30) | 執行刪除的使用者 | NULL |
| created_at | 建立時間 | DATETIME2 | 系統自動 | DEFAULT GETDATE() |
| updated_at | 更新時間 | DATETIME2 | 系統自動 | DEFAULT GETDATE() |

---

### 7. CaseEQemotionalEvaluation - 情緒評估表 (EQ)

| 欄位名稱 | 中文名稱 | 資料型態 | 說明 | 約束條件 |
|:---------|:---------|:----------|:-----|:---------|
| caseID | 個案編號 | NVARCHAR(10) | 外鍵，關聯 Cases.caseID | FOREIGN KEY, NOT NULL |
| **eq_q1** | **孩子的情緒大部分是穩定的，遇到不順心的事情或陌生環境時，孩子能隨著年紀增加逐漸適應的能力** | **INT** | **1-3分評分** | **CHECK (eq_q1 IN (1,2,3))** |
| **eq_q2** | **孩子自己有處理情緒的方式** | **INT** | **1-3分評分** | **CHECK (eq_q2 IN (1,2,3))** |
| **eq_q3** | **孩子大多數時候可以制度控制自己的負面情緒** | **INT** | **1-3分評分** | **CHECK (eq_q3 IN (1,2,3))** |
| **eq_q4** | **孩子遇到挫折時，不會長時間低落抑鬱，也不會把情緒直接遷怒到不相干的人身上(比如在學校被老師罵，回到家之後不會對手足變得特別粗暴或攻擊)** | **INT** | **1-3分評分** | **CHECK (eq_q4 IN (1,2,3))** |
| **eq_q5** | **孩子對引發情緒的原因可以理解並說明** | **INT** | **1-3分評分** | **CHECK (eq_q5 IN (1,2,3))** |
| **eq_q6** | **和過去相比，孩子遇到挫折等負面經驗，重新打起精神需花費的時間，有減少的趨勢** | **INT** | **1-3分評分** | **CHECK (eq_q6 IN (1,2,3))** |
| **eq_q7** | **孩子對自己的情緒可以描述得很清楚** | **INT** | **1-3分評分** | **CHECK (eq_q7 IN (1,2,3))** |
| deleted | 軟刪除標記 | BIT | 0=正常, 1=已刪除 | DEFAULT 0 |
| deleted_at | 刪除時間 | DATETIME2 | 軟刪除時間戳記 | NULL |
| deleted_by | 刪除者 | NVARCHAR(30) | 執行刪除的使用者 | NULL |
| created_at | 建立時間 | DATETIME2 | 系統自動 | DEFAULT GETDATE() |
| updated_at | 更新時間 | DATETIME2 | 系統自動 | DEFAULT GETDATE() |

---

### 8. FinalAssessmentSummary - 最後評估表

| 欄位名稱 | 中文名稱 | 資料型態 | 說明 | 約束條件 |
|:---------|:---------|:----------|:-----|:---------|
| caseID | 個案編號 | NVARCHAR(10) | 外鍵，關聯 Cases.caseID | FOREIGN KEY, NOT NULL |
| fq_summary | FQ 簡述評估 | TEXT | 經濟狀況總結 |  |
| hq_summary | HQ 簡述評估 | TEXT | 健康狀況總結 |  |
| iq_summary | IQ 簡述評估 | TEXT | 學業表現總結 |  |
| eq_summary | EQ 簡述評估 | TEXT | 情緒評估總結 |  |
| deleted | 軟刪除標記 | BIT | 0=正常, 1=已刪除 | DEFAULT 0 |
| deleted_at | 刪除時間 | DATETIME2 | 軟刪除時間戳記 | NULL |
| deleted_by | 刪除者 | NVARCHAR(30) | 執行刪除的使用者 | NULL |
| created_at | 建立時間 | DATETIME2 | 系統自動 | DEFAULT GETDATE() |
| updated_at | 更新時間 | DATETIME2 | 系統自動 | DEFAULT GETDATE() |

---

## 👥 個案關聯子表

### 9. CaseFamilySpecialStatus - 家庭特殊身份表

| 欄位名稱 | 中文名稱 | 資料型態 | 說明 | 約束條件 |
|:---------|:---------|:----------|:-----|:---------|
| caseID | 個案編號 | NVARCHAR(10) | 外鍵，關聯 Cases.caseID | FOREIGN KEY, NOT NULL |
| status_type_value_id | 身份類型 | INT | 外鍵，關聯 OptionSetValues.option_value_id（FAMILY_SPECIAL_STATUS_TYPE） | FOREIGN KEY, NOT NULL |
| low_income_card_number | 低收卡號 | NVARCHAR(20) | 僅當身份類型需要卡號時填寫 | NULL |
| disability_icf_code | 身心障礙ICF編碼 | NVARCHAR(100) | 僅當身份類型需要 ICF 時填寫 | NULL |
| other_description | 其他說明 | NVARCHAR(100) | 僅當身份類型需要時填寫 | NULL |
| deleted | 軟刪除標記 | BIT | 0=正常, 1=已刪除 | DEFAULT 0 |
| deleted_at | 刪除時間 | DATETIME2 | 軟刪除時間戳記 | NULL |
| deleted_by | 刪除者 | NVARCHAR(30) | 執行刪除的使用者 | NULL |
| created_at | 建立時間 | DATETIME2 | 系統自動 | DEFAULT GETDATE() |
| updated_at | 更新時間 | DATETIME2 | 系統自動 | DEFAULT GETDATE() |

---

### 10. CaseFamilyMembers - 個案家中成員表

| 欄位名稱 | 中文名稱 | 資料型態 | 說明 | 約束條件 |
|:---------|:---------|:----------|:-----|:---------|
| caseID | 個案編號 | NVARCHAR(10) | 外鍵，關聯 Cases.caseID | FOREIGN KEY, NOT NULL |
| member_type_value_id | 成員類型 | INT | 外鍵，關聯 OptionSetValues.option_value_id（FAMILY_MEMBER_TYPE） | FOREIGN KEY, NOT NULL |
| quantity | 數量 | INT | 僅當該成員類型需要數量時填寫 | NULL |
| description | 說明 | NVARCHAR(100) | 僅當該成員類型需要說明時填寫 | NULL |
| deleted | 軟刪除標記 | BIT | 0=正常, 1=已刪除 | DEFAULT 0 |
| deleted_at | 刪除時間 | DATETIME2 | 軟刪除時間戳記 | NULL |
| deleted_by | 刪除者 | NVARCHAR(30) | 執行刪除的使用者 | NULL |
| created_at | 建立時間 | DATETIME2 | 系統自動 | DEFAULT GETDATE() |
| updated_at | 更新時間 | DATETIME2 | 系統自動 | DEFAULT GETDATE() |

---

### 11. CaseFamilyMemberNotes - 家庭概況描述表

| 欄位名稱 | 中文名稱 | 資料型態 | 說明 | 約束條件 |
|:---------|:---------|:----------|:-----|:---------|
| note_id | 備註編號 | INT | 主鍵，自動遞增 | PRIMARY KEY, IDENTITY(1,1) |
| caseID | 個案編號 | NVARCHAR(10) | 外鍵，關聯 Cases.caseID | FOREIGN KEY, NOT NULL |
| member_type_value_id | 成員類型 | INT | 外鍵，關聯 OptionSetValues.option_value_id（FAMILY_MEMBER_TYPE） | FOREIGN KEY, NOT NULL |
| member_name | 成員姓名 | NVARCHAR(50) | 成員姓名（如：林敬甡、林敬玆） | NULL |
| note_content | 備註內容 | NTEXT | 完整描述內容（手打文字） | NULL |
| deleted | 軟刪除標記 | BIT | 0=正常, 1=已刪除 | DEFAULT 0 |
| deleted_at | 刪除時間 | DATETIME2 | 軟刪除時間戳記 | NULL |
| deleted_by | 刪除者 | NVARCHAR(30) | 執行刪除的使用者 | NULL |
| created_at | 建立時間 | DATETIME2 | 系統自動 | DEFAULT GETDATE() |
| updated_at | 更新時間 | DATETIME2 | 系統自動 | DEFAULT GETDATE() |

---

### 12. CaseConsultationRecords - 個案會談輔導紀錄表

| 欄位名稱 | 中文名稱 | 資料型態 | 說明 | 約束條件 |
|:---------|:---------|:----------|:-----|:---------|
| consultation_id | 會談編號 | INT | 主鍵，自動遞增 | PRIMARY KEY, IDENTITY(1,1) |
| caseID | 個案編號 | NVARCHAR(10) | 外鍵，關聯 Cases.caseID | FOREIGN KEY, NOT NULL |
| consultation_method_value_id | 訪談方式 | INT | 外鍵，關聯 OptionSetValues.option_value_id（CONSULTATION_METHOD） | FOREIGN KEY, NOT NULL |
| consultation_target_value_id | 訪談對象 | INT | 外鍵，關聯 OptionSetValues.option_value_id（CONSULTATION_TARGET） | FOREIGN KEY, NOT NULL |
| consultation_datetime | 訪談時間 | DATETIME2 | 會談/輔導時間 | NOT NULL |
| consultation_content | 會談內容 | NTEXT | 會談/輔導內容記錄 | NULL |
| deleted | 軟刪除標記 | BIT | 0=正常, 1=已刪除 | DEFAULT 0 |
| deleted_at | 刪除時間 | DATETIME2 | 軟刪除時間戳記 | NULL |
| deleted_by | 刪除者 | NVARCHAR(30) | 執行刪除的使用者 | NULL |
| created_at | 建立時間 | DATETIME2 | 系統自動 | DEFAULT GETDATE() |
| updated_at | 更新時間 | DATETIME2 | 系統自動 | DEFAULT GETDATE() |

---

### 13. CaseSocialWorkerServices - 社會工作服務內容表

| 欄位名稱 | 中文名稱 | 資料型態 | 說明 | 約束條件 |
|:---------|:---------|:----------|:-----|:---------|
| service_id | 服務編號 | INT | 主鍵，自動遞增 | PRIMARY KEY, IDENTITY(1,1) |
| caseID | 個案編號 | NVARCHAR(10) | 外鍵，關聯 Cases.caseID | FOREIGN KEY, NOT NULL |
| service_type_value_id | 服務類型 | INT | 外鍵，關聯 OptionSetValues.option_value_id（SOCIAL_SERVICE_TYPE） | FOREIGN KEY, NOT NULL |
| service_description | 服務說明 | NVARCHAR(500) | 服務詳細說明或備註 | NULL |
| service_date | 服務日期 | DATE | 服務提供日期 | NULL |
| service_provider | 服務提供者 | NVARCHAR(50) | 提供服務的社工或單位 | NULL |
| deleted | 軟刪除標記 | BIT | 0=正常, 1=已刪除 | DEFAULT 0 |
| deleted_at | 刪除時間 | DATETIME2 | 軟刪除時間戳記 | NULL |
| deleted_by | 刪除者 | NVARCHAR(30) | 執行刪除的使用者 | NULL |
| created_at | 建立時間 | DATETIME2 | 系統自動 | DEFAULT GETDATE() |
| updated_at | 更新時間 | DATETIME2 | 系統自動 | DEFAULT GETDATE() |

---

## 🏫 選項參考表

### 14. FamilyStructureTypes - 家庭結構類型選項表

| 欄位名稱 | 中文名稱 | 資料型態 | 說明 | 約束條件 |
|:---------|:---------|:----------|:-----|:---------|
| structure_type_id | 結構類型編號 | INT | 主鍵，自動遞增 | PRIMARY KEY, IDENTITY(1,1) |
| structure_code | 結構代碼 | NVARCHAR(20) | 如 GENERAL, SINGLE_FATHER 等 | NOT NULL, UNIQUE |
| structure_name | 結構名稱 | NVARCHAR(50) | 如 一般家庭、單親家庭（父）等 | NOT NULL |
| needs_description | 需要描述 | BIT | 是否需要額外描述 | DEFAULT 0 |

---

### 15. Nationalities - 國籍選項表

| 欄位名稱 | 中文名稱 | 資料型態 | 說明 | 約束條件 |
|:---------|:---------|:----------|:-----|:---------|
| nationality_id | 國籍編號 | INT | 主鍵，自動遞增 | PRIMARY KEY, IDENTITY(1,1) |
| nationality_name | 國籍名稱 | NVARCHAR(20) | 國籍完整名稱 | NOT NULL, UNIQUE |
| nationality_code | 國籍代碼 | NVARCHAR(10) | 如 TW, VN, TH 等 | NOT NULL, UNIQUE |

---

### 16. Schools - 學校選項表

| 欄位名稱 | 中文名稱 | 資料型態 | 說明 | 約束條件 |
|:---------|:---------|:----------|:-----|:---------|
| school_id | 學校編號 | INT | 主鍵，自動遞增 | PRIMARY KEY, IDENTITY(1,1) |
| school_name | 學校名稱 | NVARCHAR(50) | 學校完整名稱 | NOT NULL, UNIQUE |
| school_type | 學校類型 | NVARCHAR(20) | 國小/國中/高中/大學/其他 | NOT NULL |

---

### 17. Cities - 城市選項表

| 欄位名稱 | 中文名稱 | 資料型態 | 說明 | 約束條件 |
|:---------|:---------|:----------|:-----|:---------|
| city_id | 城市編號 | INT | 主鍵，自動遞增 | PRIMARY KEY, IDENTITY(1,1) |
| city_name | 城市名稱 | NVARCHAR(10) | 城市完整名稱 | NOT NULL, UNIQUE |

---

### 18. Districts - 地區選項表

| 欄位名稱 | 中文名稱 | 資料型態 | 說明 | 約束條件 |
|:---------|:---------|:----------|:-----|:---------|
| district_id | 地區編號 | INT | 主鍵，自動遞增 | PRIMARY KEY, IDENTITY(1,1) |
| district_name | 地區名稱 | NVARCHAR(10) | 地區完整名稱 | NOT NULL, UNIQUE |
| city_id | 所屬城市 | INT | 外鍵，關聯 Cities.city_id | FOREIGN KEY, NOT NULL |

---

### 19. OptionSets - 通用選項集定義表

| 欄位名稱 | 中文名稱 | 資料型態 | 說明 | 約束條件 |
|:---------|:---------|:----------|:-----|:---------|
| option_set_id | 選項集編號 | INT | 主鍵，自動遞增 | PRIMARY KEY, IDENTITY(1,1) |
| option_key | 選項集鍵值 | NVARCHAR(50) | 如 FAMILY_SPECIAL_STATUS_TYPE、FAMILY_MEMBER_TYPE | NOT NULL, UNIQUE |
| option_set_name | 選項集名稱 | NVARCHAR(50) | 中文名稱 | NOT NULL |

---

### 20. OptionSetValues - 通用選項值表

| 欄位名稱 | 中文名稱 | 資料型態 | 說明 | 約束條件 |
|:---------|:---------|:----------|:-----|:---------|
| option_value_id | 選項值編號 | INT | 主鍵，自動遞增 | PRIMARY KEY, IDENTITY(1,1) |
| option_set_id | 所屬選項集 | INT | 外鍵，關聯 OptionSets.option_set_id | FOREIGN KEY, NOT NULL |
| value_code | 選項代碼 | NVARCHAR(30) | 例如 LOW_INCOME、DISABLED、FATHER 等 | NOT NULL |
| value_name | 選項名稱 | NVARCHAR(50) | 顯示名稱 | NOT NULL |

---

## 📚 歷史記錄表

### 21. CaseHistory - 個案歷史記錄表

| 欄位名稱 | 中文名稱 | 資料型態 | 說明 | 約束條件 |
|:---------|:---------|:----------|:-----|:---------|
| history_id | 歷史記錄編號 | BIGINT | 主鍵，自動遞增 | PRIMARY KEY, IDENTITY(1,1) |
| caseID | 個案編號 | NVARCHAR(10) | 外鍵，關聯 Cases.caseID | FOREIGN KEY, NOT NULL |
| version_number | 版本號 | INT | 版本編號 | NOT NULL |
| change_type | 變更類型 | NVARCHAR(20) | INSERT/UPDATE/DELETE | NOT NULL |
| field_name | 欄位名稱 | NVARCHAR(100) | 被修改的欄位 |  |
| old_value | 舊值 | NVARCHAR(MAX) | 修改前的值 |  |
| new_value | 新值 | NVARCHAR(MAX) | 修改後的值 |  |
| change_reason | 變更原因 | NVARCHAR(500) | 變更說明 |  |
| changed_by | 變更者 | NVARCHAR(30) | 執行變更的使用者 |  |
| changed_at | 變更時間 | DATETIME2 | 變更時間戳記 | DEFAULT GETDATE() |
| ip_address | IP位址 | NVARCHAR(45) | 變更來源IP |  |
| user_agent | 使用者代理 | NVARCHAR(500) | 瀏覽器或應用程式資訊 |  |

---

### 22. CaseDetailHistory - 個案詳細資料歷史記錄表

| 欄位名稱 | 中文名稱 | 資料型態 | 說明 | 約束條件 |
|:---------|:---------|:----------|:-----|:---------|
| history_id | 歷史記錄編號 | BIGINT | 主鍵，自動遞增 | PRIMARY KEY, IDENTITY(1,1) |
| caseID | 個案編號 | NVARCHAR(10) | 外鍵，關聯 Cases.caseID | FOREIGN KEY, NOT NULL |
| version_number | 版本號 | INT | 版本編號 | NOT NULL |
| change_type | 變更類型 | NVARCHAR(20) | INSERT/UPDATE/DELETE | NOT NULL |
| field_name | 欄位名稱 | NVARCHAR(100) | 被修改的欄位 |  |
| old_value | 舊值 | NVARCHAR(MAX) | 修改前的值 |  |
| new_value | 新值 | NVARCHAR(MAX) | 修改後的值 |  |
| change_reason | 變更原因 | NVARCHAR(500) | 變更說明 |  |
| changed_by | 變更者 | NVARCHAR(30) | 執行變更的使用者 |  |
| changed_at | 變更時間 | DATETIME2 | 變更時間戳記 | DEFAULT GETDATE() |
| ip_address | IP位址 | NVARCHAR(45) | 變更來源IP |  |
| user_agent | 使用者代理 | NVARCHAR(500) | 瀏覽器或應用程式資訊 |  |

---

## 🔍 稽核與活動記錄

### 23. DataChangeLog - 資料變更紀錄表

| 欄位名稱 | 中文名稱 | 資料型態 | 說明 | 約束條件 |
|:---------|:---------|:----------|:-----|:---------|
| logID | 紀錄編號 | BIGINT | 主鍵，自動遞增 | PRIMARY KEY, IDENTITY(1,1) |
| table_name | 資料表名稱 | NVARCHAR(100) | 被修改的資料表 | NOT NULL |
| record_id | 記錄ID | NVARCHAR(50) | 被修改記錄的主鍵 |  |
| operation_type | 操作類型 | NVARCHAR(20) | INSERT/UPDATE/DELETE | NOT NULL |
| field_name | 欄位名稱 | NVARCHAR(100) | 被修改的欄位 |  |
| old_value | 舊值 | NVARCHAR(MAX) | 修改前的值 |  |
| new_value | 新值 | NVARCHAR(MAX) | 修改後的值 |  |
| changed_by | 變更者 | NVARCHAR(30) | 執行變更的使用者 |  |
| changed_at | 變更時間 | DATETIME2 | 變更時間戳記 | DEFAULT GETDATE() |
| ip_address | IP位址 | NVARCHAR(45) | 變更來源IP |  |
| user_agent | 使用者代理 | NVARCHAR(500) | 瀏覽器或應用程式資訊 |  |
| created_at | 建檔時間 | DATETIME2 | 系統自動 | DEFAULT GETDATE() |
| updated_at | 更新時間 | DATETIME2 | 系統自動 | DEFAULT GETDATE() |

---

### 24. UserActivityLog - 使用者活動紀錄表

| 欄位名稱 | 中文名稱 | 資料型態 | 說明 | 約束條件 |
|:---------|:---------|:----------|:-----|:---------|
| activityID | 活動編號 | BIGINT | 主鍵，自動遞增 | PRIMARY KEY, IDENTITY(1,1) |
| user_id | 使用者ID | NVARCHAR(50) | 執行活動的使用者 | NOT NULL |
| activity_type | 活動類型 | NVARCHAR(50) | 登入/登出/查詢/新增/修改/刪除 | NOT NULL |
| activity_description | 活動描述 | NVARCHAR(500) | 活動詳細說明 |  |
| target_table | 目標資料表 | NVARCHAR(100) | 操作的資料表 |  |
| target_record_id | 目標記錄ID | NVARCHAR(50) | 操作的記錄ID |  |
| ip_address | IP位址 | NVARCHAR(45) | 活動來源IP |  |
| user_agent | 使用者代理 | NVARCHAR(500) | 瀏覽器或應用程式資訊 |  |
| created_at | 紀錄時間 | DATETIME2 | 系統自動記錄 | DEFAULT GETDATE() |

---

## 📋 通用選項集初始化資料

### OptionSets 表資料

| option_set_id | option_key | option_set_name |
|:-------------|:-----------|:----------------|
| 1 | FAMILY_SPECIAL_STATUS_TYPE | 家庭特殊身份類型 |
| 2 | MARITAL_STATUS | 婚姻狀況 |
| 3 | EDUCATION_LEVEL | 教育程度 |
| 4 | CASE_SOURCE | 個案來源 |
| 5 | FAMILY_MEMBER_TYPE | 家庭成員類型 |
| 6 | CONTACT_RELATION | 聯絡人關係 |
| 7 | HELP_EXPERIENCE | 求助經驗 |
| 8 | CONSULTATION_METHOD | 訪談方式 |
| 9 | CONSULTATION_TARGET | 訪談對象 |
| 10 | SOCIAL_SERVICE_TYPE | 社會工作服務類型 |
| 11 | RESIDENCE_TYPE | 居住地型態 |
| 12 | CARE_GIVER_ROLE | 照顧者角色 |

### OptionSetValues 表資料

#### 1. 家庭特殊身份類型 (FAMILY_SPECIAL_STATUS_TYPE)
| option_value_id | option_set_id | value_code | value_name |
|:---------------|:-------------|:-----------|:-----------|
| 1 | 1 | NONE | 無 |
| 2 | 1 | LOW_INCOME | 低收入戶 |
| 3 | 1 | MID_LOW_INCOME | 中低收入戶 |
| 4 | 1 | NEAR_POOR | 近貧戶 |
| 5 | 1 | MAJOR_ILLNESS | 重大傷病 |
| 6 | 1 | DISABLED | 身心障礙 |
| 7 | 1 | INDIGENOUS | 原住民身分 |
| 8 | 1 | OTHER | 其他 |

#### 2. 婚姻狀況 (MARITAL_STATUS)
| option_value_id | option_set_id | value_code | value_name |
|:---------------|:-------------|:-----------|:-----------|
| 9 | 2 | SINGLE | 未婚 |
| 10 | 2 | MARRIED | 結婚 |
| 11 | 2 | DIVORCED | 離婚 |
| 12 | 2 | WIDOWED | 喪偶 |
| 13 | 2 | SEPARATED | 分居 |
| 14 | 2 | COHABITING | 同居 |
| 15 | 2 | OTHER | 其他 |

#### 3. 教育程度 (EDUCATION_LEVEL)
| option_value_id | option_set_id | value_code | value_name |
|:---------------|:-------------|:-----------|:-----------|
| 16 | 3 | BELOW_ELEMENTARY | 小學以下 |
| 17 | 3 | ELEMENTARY | 小學 |
| 18 | 3 | JUNIOR_HIGH | 國中 |
| 19 | 3 | SENIOR_HIGH | 高中職 |
| 20 | 3 | COLLEGE | 專科 |
| 21 | 3 | UNIVERSITY | 大學 |
| 22 | 3 | MASTER_ABOVE | 碩士以上 |
| 23 | 3 | UNKNOWN | 不詳 |

#### 4. 個案來源 (CASE_SOURCE)
| option_value_id | option_set_id | value_code | value_name |
|:---------------|:-------------|:-----------|:-----------|
| 24 | 4 | SELF_REFERRAL | 自行求助 |
| 25 | 4 | FAMILY_FRIENDS | 親友 |
| 26 | 4 | SCHOOL_TEACHER | 學校老師 |
| 27 | 4 | MEDICAL_INSTITUTION | 醫療院所 |
| 28 | 4 | SOCIAL_WELFARE_CENTER | 社福中心 |
| 29 | 4 | CIVIL_ORGANIZATION | 民間團體 |
| 30 | 4 | CASE_MANAGER | 個管社工 |
| 31 | 4 | CANLOVE_FAMILY | 肯愛認養家庭 |
| 32 | 4 | OTHER | 其他 |

#### 5. 家庭成員類型 (FAMILY_MEMBER_TYPE)
| option_value_id | option_set_id | value_code | value_name |
|:---------------|:-------------|:-----------|:-----------|
| 33 | 5 | FATHER | 父 |
| 34 | 5 | MOTHER | 母 |
| 35 | 5 | GRANDFATHER | 祖父 |
| 36 | 5 | GRANDMOTHER | 祖母 |
| 37 | 5 | BROTHER | 兄弟 |
| 38 | 5 | SISTER | 姊妹 |
| 39 | 5 | UNCLE | 叔伯 |
| 40 | 5 | AUNT | 姑姨 |
| 41 | 5 | OTHER | 其他 |

#### 6. 聯絡人關係 (CONTACT_RELATION)
| option_value_id | option_set_id | value_code | value_name |
|:---------------|:-------------|:-----------|:-----------|
| 42 | 6 | FATHER | 父 |
| 43 | 6 | MOTHER | 母 |
| 44 | 6 | GRANDFATHER | 祖父 |
| 45 | 6 | GRANDMOTHER | 祖母 |
| 46 | 6 | BROTHER | 兄弟 |
| 47 | 6 | SISTER | 姊妹 |
| 48 | 6 | UNCLE | 叔伯 |
| 49 | 6 | AUNT | 姑姨 |
| 50 | 6 | TEACHER | 老師 |
| 51 | 6 | NEIGHBOR | 鄰居 |
| 52 | 6 | FRIEND | 朋友 |
| 53 | 6 | OTHER | 其他 |

#### 7. 求助經驗 (HELP_EXPERIENCE)
| option_value_id | option_set_id | value_code | value_name |
|:---------------|:-------------|:-----------|:-----------|
| 54 | 7 | NONE | 無 |
| 55 | 7 | SOCIAL_WELFARE_WITH_CASE_MANAGER | 社會福利（有個管） |
| 56 | 7 | SOCIAL_WELFARE_UNKNOWN | 社會福利（不詳） |
| 57 | 7 | HOSPITAL | 醫院 |
| 58 | 7 | FAMILY_FRIENDS | 親友 |
| 59 | 7 | SCHOOL_COUNSELING | 學校輔導室 |
| 60 | 7 | OTHER | 其他 |

#### 8. 訪談方式 (CONSULTATION_METHOD)
| option_value_id | option_set_id | value_code | value_name |
|:---------------|:-------------|:-----------|:-----------|
| 61 | 8 | HOME_VISIT | 家庭訪視 |
| 62 | 8 | PHONE_INTERVIEW | 電話訪問 |
| 63 | 8 | INSTITUTION_MEETING | 機構會談 |

#### 9. 訪談對象 (CONSULTATION_TARGET)
| option_value_id | option_set_id | value_code | value_name |
|:---------------|:-------------|:-----------|:-----------|
| 64 | 9 | FATHER | 父 |
| 65 | 9 | MOTHER | 母 |
| 66 | 9 | GRANDFATHER | 祖父 |
| 67 | 9 | GRANDMOTHER | 祖母 |
| 68 | 9 | MATERNAL_GRANDFATHER | 外祖父 |
| 69 | 9 | MATERNAL_GRANDMOTHER | 外祖母 |
| 70 | 9 | OTHER | 其他 |

#### 10. 社會工作服務類型 (SOCIAL_SERVICE_TYPE)
| option_value_id | option_set_id | value_code | value_name |
|:---------------|:-------------|:-----------|:-----------|
| 71 | 10 | FAMILY_ASSESSMENT | 家庭評估 |
| 72 | 10 | ADOPTION_SERVICE | 認養服務 |
| 73 | 10 | EMOTIONAL_SUPPORT | 情緒支持 |
| 74 | 10 | PARENTING_EDUCATION | 親職教育 |
| 75 | 10 | MENTAL_HEALTH_EDUCATION | 精神疾病(憂鬱症)基礎衛教 |
| 76 | 10 | SUPPLY_PROVISION | 物資提供 |
| 77 | 10 | RESOURCE_LIAISON | 資源聯繫(其他社福單位) |
| 78 | 10 | WELFARE_RESOURCE_EXPLANATION | 福利資源說明 |
| 79 | 10 | REFERRAL_SERVICE | 轉介服務 |
| 80 | 10 | OTHER | 其他 |

#### 11. 居住地型態 (RESIDENCE_TYPE)
| option_value_id | option_set_id | value_code | value_name |
|:---------------|:-------------|:-----------|:-----------|
| 81 | 11 | OWNED | 自有 |
| 82 | 11 | RENTED | 租屋 |
| 83 | 11 | LIVING_WITH_RELATIVES | 與親友同住（房屋為親友所有） |
| 84 | 11 | BORROWED_FROM_RELATIVES | 親友房屋借住 |
| 85 | 11 | OTHER | 其他 |

#### 12. 照顧者角色 (CARE_GIVER_ROLE)
| option_value_id | option_set_id | value_code | value_name |
|:---------------|:-------------|:-----------|:-----------|
| 86 | 12 | FATHER | 父 |
| 87 | 12 | MOTHER | 母 |
| 88 | 12 | GRANDFATHER | 祖父 |
| 89 | 12 | GRANDMOTHER | 祖母 |
| 90 | 12 | BROTHER | 兄弟 |
| 91 | 12 | SISTER | 姊妹 |
| 92 | 12 | UNCLE | 叔伯 |
| 93 | 12 | AUNT | 姑姨 |
| 94 | 12 | CASE_SELF | 被照顧者本身 |
| 95 | 12 | OTHER | 其他 |