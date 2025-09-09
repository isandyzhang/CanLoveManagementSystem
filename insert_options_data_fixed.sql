-- =============================================
-- NGO 個案管理系統 - 選項表初始化數據（修正版本）
-- 適用於 DBeaver SQL 腳本 - 已修正字串長度問題
-- =============================================

-- 1. 插入家庭結構類型選項
MERGE FamilyStructureTypes AS target
USING (VALUES 
    ('GENERAL', '一般家庭', 0),
    ('SINGLE_FATHER', '單親家庭（父）', 0),
    ('SINGLE_MOTHER', '單親家庭（母）', 0),
    ('GRANDPARENT', '隔代教養', 0),
    ('FOSTER', '寄養家庭', 0),
    ('ADOPTIVE', '收養家庭', 0),
    ('OTHER', '其他', 1)
) AS source (structure_code, structure_name, needs_description)
ON target.structure_code = source.structure_code
WHEN NOT MATCHED THEN
    INSERT (structure_code, structure_name, needs_description)
    VALUES (source.structure_code, source.structure_name, source.needs_description);

-- 2. 插入國籍選項
MERGE Nationalities AS target
USING (VALUES 
    ('中華民國', 'TW'),
    ('越南', 'VN'),
    ('泰國', 'TH'),
    ('印尼', 'ID'),
    ('菲律賓', 'PH'),
    ('馬來西亞', 'MY'),
    ('新加坡', 'SG'),
    ('日本', 'JP'),
    ('韓國', 'KR'),
    ('美國', 'US'),
    ('其他', 'OTHER')
) AS source (nationality_name, nationality_code)
ON target.nationality_code = source.nationality_code
WHEN NOT MATCHED THEN
    INSERT (nationality_name, nationality_code)
    VALUES (source.nationality_name, source.nationality_code);

-- 3. 插入學校選項（台北市國小為主）
MERGE Schools AS target
USING (VALUES 
    -- 台北市國小
    ('台北市立建國國民小學', '國小'),
    ('台北市立成功國民小學', '國小'),
    ('台北市立中山國民小學', '國小'),
    ('台北市立中正國民小學', '國小'),
    ('台北市立大同國民小學', '國小'),
    ('台北市立松山國民小學', '國小'),
    ('台北市立大安國民小學', '國小'),
    ('台北市立萬華國民小學', '國小'),
    ('台北市立信義國民小學', '國小'),
    ('台北市立士林國民小學', '國小'),
    ('台北市立北投國民小學', '國小'),
    ('台北市立內湖國民小學', '國小'),
    ('台北市立南港國民小學', '國小'),
    ('台北市立文山國民小學', '國小'),
    ('台北市立敦化國民小學', '國小'),
    ('台北市立民生國民小學', '國小'),
    ('台北市立復興國民小學', '國小'),
    ('台北市立懷生國民小學', '國小'),
    ('台北市立仁愛國民小學', '國小'),
    ('台北市立光復國民小學', '國小'),
    -- 台北市國中
    ('台北市立建國國民中學', '國中'),
    ('台北市立成功國民中學', '國中'),
    ('台北市立中山國民中學', '國中'),
    ('台北市立中正國民中學', '國中'),
    ('台北市立大同國民中學', '國中'),
    ('台北市立松山國民中學', '國中'),
    ('台北市立大安國民中學', '國中'),
    ('台北市立萬華國民中學', '國中'),
    ('台北市立信義國民中學', '國中'),
    ('台北市立士林國民中學', '國中'),
    ('台北市立北投國民中學', '國中'),
    ('台北市立內湖國民中學', '國中'),
    ('台北市立南港國民中學', '國中'),
    ('台北市立文山國民中學', '國中'),
    -- 大學
    ('國立台灣大學', '大學'),
    ('國立政治大學', '大學'),
    ('國立清華大學', '大學'),
    ('國立台灣師範大學', '大學'),
    ('國立台北大學', '大學'),
    ('國立台北科技大學', '大學'),
    ('國立台灣科技大學', '大學'),
    ('其他', '其他')
) AS source (school_name, school_type)
ON target.school_name = source.school_name
WHEN NOT MATCHED THEN
    INSERT (school_name, school_type)
    VALUES (source.school_name, source.school_type);

-- 4. 插入選項集定義
MERGE OptionSets AS target
USING (VALUES 
    ('FAMILY_SPECIAL_STATUS_TYPE', '家庭特殊身份類型'),
    ('MARITAL_STATUS', '婚姻狀況'),
    ('EDUCATION_LEVEL', '教育程度'),
    ('CASE_SOURCE', '個案來源'),
    ('FAMILY_MEMBER_TYPE', '家庭成員類型'),
    ('CONTACT_RELATION', '聯絡人關係'),
    ('HELP_EXPERIENCE', '求助經驗'),
    ('CONSULTATION_METHOD', '訪談方式'),
    ('CONSULTATION_TARGET', '訪談對象'),
    ('SOCIAL_SERVICE_TYPE', '社會工作服務類型'),
    ('RESIDENCE_TYPE', '居住地型態'),
    ('CARE_GIVER_ROLE', '照顧者角色')
) AS source (option_key, option_set_name)
ON target.option_key = source.option_key
WHEN NOT MATCHED THEN
    INSERT (option_key, option_set_name)
    VALUES (source.option_key, source.option_set_name);

-- 5. 插入選項值數據
-- 5.1 家庭特殊身份類型
MERGE OptionSetValues AS target
USING (VALUES 
    (1, 'NONE', '無'),
    (1, 'LOW_INCOME', '低收入戶'),
    (1, 'MID_LOW_INCOME', '中低收入戶'),
    (1, 'NEAR_POOR', '近貧戶'),
    (1, 'MAJOR_ILLNESS', '重大傷病'),
    (1, 'DISABLED', '身心障礙'),
    (1, 'INDIGENOUS', '原住民身分'),
    (1, 'OTHER', '其他')
) AS source (option_set_id, value_code, value_name)
ON target.option_set_id = source.option_set_id AND target.value_code = source.value_code
WHEN NOT MATCHED THEN
    INSERT (option_set_id, value_code, value_name)
    VALUES (source.option_set_id, source.value_code, source.value_name);

-- 5.2 婚姻狀況
MERGE OptionSetValues AS target
USING (VALUES 
    (2, 'SINGLE', '未婚'),
    (2, 'MARRIED', '結婚'),
    (2, 'DIVORCED', '離婚'),
    (2, 'WIDOWED', '喪偶'),
    (2, 'SEPARATED', '分居'),
    (2, 'COHABITING', '同居'),
    (2, 'OTHER', '其他')
) AS source (option_set_id, value_code, value_name)
ON target.option_set_id = source.option_set_id AND target.value_code = source.value_code
WHEN NOT MATCHED THEN
    INSERT (option_set_id, value_code, value_name)
    VALUES (source.option_set_id, source.value_code, source.value_name);

-- 5.3 教育程度
MERGE OptionSetValues AS target
USING (VALUES 
    (3, 'BELOW_ELEMENTARY', '小學以下'),
    (3, 'ELEMENTARY', '小學'),
    (3, 'JUNIOR_HIGH', '國中'),
    (3, 'SENIOR_HIGH', '高中職'),
    (3, 'COLLEGE', '專科'),
    (3, 'UNIVERSITY', '大學'),
    (3, 'MASTER_ABOVE', '碩士以上'),
    (3, 'UNKNOWN', '不詳')
) AS source (option_set_id, value_code, value_name)
ON target.option_set_id = source.option_set_id AND target.value_code = source.value_code
WHEN NOT MATCHED THEN
    INSERT (option_set_id, value_code, value_name)
    VALUES (source.option_set_id, source.value_code, source.value_name);

-- 5.4 個案來源
MERGE OptionSetValues AS target
USING (VALUES 
    (4, 'SELF_REFERRAL', '自行求助'),
    (4, 'FAMILY_FRIENDS', '親友'),
    (4, 'SCHOOL_TEACHER', '學校老師'),
    (4, 'MEDICAL_INSTITUTION', '醫療院所'),
    (4, 'SOCIAL_WELFARE_CENTER', '社福中心'),
    (4, 'CIVIL_ORGANIZATION', '民間團體'),
    (4, 'CASE_MANAGER', '個管社工'),
    (4, 'CANLOVE_FAMILY', '肯愛認養家庭'),
    (4, 'OTHER', '其他')
) AS source (option_set_id, value_code, value_name)
ON target.option_set_id = source.option_set_id AND target.value_code = source.value_code
WHEN NOT MATCHED THEN
    INSERT (option_set_id, value_code, value_name)
    VALUES (source.option_set_id, source.value_code, source.value_name);

-- 5.5 家庭成員類型
MERGE OptionSetValues AS target
USING (VALUES 
    (5, 'FATHER', '父'),
    (5, 'MOTHER', '母'),
    (5, 'GRANDFATHER', '祖父'),
    (5, 'GRANDMOTHER', '祖母'),
    (5, 'BROTHER', '兄弟'),
    (5, 'SISTER', '姊妹'),
    (5, 'UNCLE', '叔伯'),
    (5, 'AUNT', '姑姨'),
    (5, 'OTHER', '其他')
) AS source (option_set_id, value_code, value_name)
ON target.option_set_id = source.option_set_id AND target.value_code = source.value_code
WHEN NOT MATCHED THEN
    INSERT (option_set_id, value_code, value_name)
    VALUES (source.option_set_id, source.value_code, source.value_name);

-- 5.6 聯絡人關係
MERGE OptionSetValues AS target
USING (VALUES 
    (6, 'FATHER', '父'),
    (6, 'MOTHER', '母'),
    (6, 'GRANDFATHER', '祖父'),
    (6, 'GRANDMOTHER', '祖母'),
    (6, 'BROTHER', '兄弟'),
    (6, 'SISTER', '姊妹'),
    (6, 'UNCLE', '叔伯'),
    (6, 'AUNT', '姑姨'),
    (6, 'TEACHER', '老師'),
    (6, 'NEIGHBOR', '鄰居'),
    (6, 'FRIEND', '朋友'),
    (6, 'OTHER', '其他')
) AS source (option_set_id, value_code, value_name)
ON target.option_set_id = source.option_set_id AND target.value_code = source.value_code
WHEN NOT MATCHED THEN
    INSERT (option_set_id, value_code, value_name)
    VALUES (source.option_set_id, source.value_code, source.value_name);

-- 5.7 求助經驗（已修正過長字串）
MERGE OptionSetValues AS target
USING (VALUES 
    (7, 'NONE', '無'),
    (7, 'SOCIAL_WELFARE_WITH_MANAGER', '社會福利（有個管）'),
    (7, 'SOCIAL_WELFARE_UNKNOWN', '社會福利（不詳）'),
    (7, 'HOSPITAL', '醫院'),
    (7, 'FAMILY_FRIENDS', '親友'),
    (7, 'SCHOOL_COUNSELING', '學校輔導室'),
    (7, 'OTHER', '其他')
) AS source (option_set_id, value_code, value_name)
ON target.option_set_id = source.option_set_id AND target.value_code = source.value_code
WHEN NOT MATCHED THEN
    INSERT (option_set_id, value_code, value_name)
    VALUES (source.option_set_id, source.value_code, source.value_name);

-- 5.8 訪談方式
MERGE OptionSetValues AS target
USING (VALUES 
    (8, 'HOME_VISIT', '家庭訪視'),
    (8, 'PHONE_INTERVIEW', '電話訪問'),
    (8, 'INSTITUTION_MEETING', '機構會談')
) AS source (option_set_id, value_code, value_name)
ON target.option_set_id = source.option_set_id AND target.value_code = source.value_code
WHEN NOT MATCHED THEN
    INSERT (option_set_id, value_code, value_name)
    VALUES (source.option_set_id, source.value_code, source.value_name);

-- 5.9 訪談對象
MERGE OptionSetValues AS target
USING (VALUES 
    (9, 'FATHER', '父'),
    (9, 'MOTHER', '母'),
    (9, 'GRANDFATHER', '祖父'),
    (9, 'GRANDMOTHER', '祖母'),
    (9, 'MATERNAL_GRANDFATHER', '外祖父'),
    (9, 'MATERNAL_GRANDMOTHER', '外祖母'),
    (9, 'OTHER', '其他')
) AS source (option_set_id, value_code, value_name)
ON target.option_set_id = source.option_set_id AND target.value_code = source.value_code
WHEN NOT MATCHED THEN
    INSERT (option_set_id, value_code, value_name)
    VALUES (source.option_set_id, source.value_code, source.value_name);

-- 5.10 社會工作服務類型
MERGE OptionSetValues AS target
USING (VALUES 
    (10, 'FAMILY_ASSESSMENT', '家庭評估'),
    (10, 'ADOPTION_SERVICE', '認養服務'),
    (10, 'EMOTIONAL_SUPPORT', '情緒支持'),
    (10, 'PARENTING_EDUCATION', '親職教育'),
    (10, 'MENTAL_HEALTH_EDUCATION', '精神疾病(憂鬱症)基礎衛教'),
    (10, 'SUPPLY_PROVISION', '物資提供'),
    (10, 'RESOURCE_LIAISON', '資源聯繫(其他社福單位)'),
    (10, 'WELFARE_RESOURCE_EXPLANATION', '福利資源說明'),
    (10, 'REFERRAL_SERVICE', '轉介服務'),
    (10, 'OTHER', '其他')
) AS source (option_set_id, value_code, value_name)
ON target.option_set_id = source.option_set_id AND target.value_code = source.value_code
WHEN NOT MATCHED THEN
    INSERT (option_set_id, value_code, value_name)
    VALUES (source.option_set_id, source.value_code, source.value_name);

-- 5.11 居住地型態
MERGE OptionSetValues AS target
USING (VALUES 
    (11, 'OWNED', '自有'),
    (11, 'RENTED', '租屋'),
    (11, 'LIVING_WITH_RELATIVES', '與親友同住（房屋為親友所有）'),
    (11, 'BORROWED_FROM_RELATIVES', '親友房屋借住'),
    (11, 'OTHER', '其他')
) AS source (option_set_id, value_code, value_name)
ON target.option_set_id = source.option_set_id AND target.value_code = source.value_code
WHEN NOT MATCHED THEN
    INSERT (option_set_id, value_code, value_name)
    VALUES (source.option_set_id, source.value_code, source.value_name);

-- 5.12 照顧者角色
MERGE OptionSetValues AS target
USING (VALUES 
    (12, 'FATHER', '父'),
    (12, 'MOTHER', '母'),
    (12, 'GRANDFATHER', '祖父'),
    (12, 'GRANDMOTHER', '祖母'),
    (12, 'BROTHER', '兄弟'),
    (12, 'SISTER', '姊妹'),
    (12, 'UNCLE', '叔伯'),
    (12, 'AUNT', '姑姨'),
    (12, 'CASE_SELF', '被照顧者本身'),
    (12, 'OTHER', '其他')
) AS source (option_set_id, value_code, value_name)
ON target.option_set_id = source.option_set_id AND target.value_code = source.value_code
WHEN NOT MATCHED THEN
    INSERT (option_set_id, value_code, value_name)
    VALUES (source.option_set_id, source.value_code, source.value_name);

-- =============================================
-- 完成選項表數據插入（修正版本）
-- 已修正：SOCIAL_WELFARE_WITH_CASE_MANAGER -> SOCIAL_WELFARE_WITH_MANAGER
-- =============================================
