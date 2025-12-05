/**
 * Case Form Common JavaScript
 * 共用於 Create 和 Edit 頁面的 JavaScript 功能
 */

// 城市/地區聯動功能
function initCityDistrict(districtsByCity, preserveDistrictId = null) {
    const citySelect = document.getElementById('citySelect');
    const districtSelect = document.getElementById('districtSelect');
    
    if (!citySelect || !districtSelect) return;
    
    function loadDistrictsForCity(cityId, preserveId = null) {
        const cityIdKey = cityId ? parseInt(cityId) : null;
        
        if (preserveId === null) {
            preserveId = districtSelect.value;
        }
        
        // 清空地區選項
        districtSelect.innerHTML = '<option value="">請選擇地區</option>';
        districtSelect.disabled = !cityIdKey;
        
        if (cityIdKey && districtsByCity && districtsByCity[cityIdKey]) {
            districtsByCity[cityIdKey].forEach(district => {
                const option = document.createElement('option');
                option.value = district.districtId;
                option.textContent = district.districtName;
                if (preserveId && preserveId == district.districtId) {
                    option.selected = true;
                }
                districtSelect.appendChild(option);
            });
            districtSelect.disabled = false;
        }
    }
    
    // 頁面載入時，如果城市已選擇，自動載入對應的地區選項
    const selectedCityId = citySelect.value;
    if (selectedCityId) {
        const loadDistricts = () => {
            loadDistrictsForCity(selectedCityId, preserveDistrictId || districtSelect.getAttribute('data-current-district-id'));
        };
        
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function() {
                setTimeout(loadDistricts, 100);
            });
        } else {
            setTimeout(loadDistricts, 100);
        }
    }
    
    // 城市選擇變更事件
    citySelect.addEventListener('change', function() {
        loadDistrictsForCity(this.value);
    });
}

// 新增學校功能
function initSchoolAdd() {
    const addSchoolBtn = document.getElementById('addSchoolBtn');
    const addSchoolForm = document.getElementById('addSchoolForm');
    const schoolSelect = document.getElementById('schoolSelect');
    const newSchoolName = document.getElementById('newSchoolName');
    const newSchoolType = document.getElementById('newSchoolType');
    const submitNewSchool = document.getElementById('submitNewSchool');
    const cancelAddSchool = document.getElementById('cancelAddSchool');

    if (!addSchoolBtn || !addSchoolForm || !schoolSelect) return;

    // 顯示新增學校表單
    addSchoolBtn.addEventListener('click', function() {
        addSchoolForm.style.display = 'block';
        if (newSchoolName) newSchoolName.focus();
    });

    // 取消新增學校
    if (cancelAddSchool) {
        cancelAddSchool.addEventListener('click', function() {
            addSchoolForm.style.display = 'none';
            if (newSchoolName) newSchoolName.value = '';
            if (newSchoolType) newSchoolType.value = '';
        });
    }

    // 送出新增學校
    if (submitNewSchool) {
        submitNewSchool.addEventListener('click', async function() {
            const name = newSchoolName.value.trim();
            const type = newSchoolType.value.trim();

            if (!name) {
                alert('請輸入學校名稱');
                newSchoolName.focus();
                return;
            }

            if (!type) {
                alert('請選擇學校類型');
                if (newSchoolType) newSchoolType.focus();
                return;
            }

            // 禁用按鈕防止重複提交
            submitNewSchool.disabled = true;
            submitNewSchool.innerHTML = '<i class="fas fa-spinner fa-spin"></i> 新增中...';

            try {
                const resp = await fetch('/api/school', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ schoolName: name, schoolType: type })
                });
                const result = await resp.json();

                if (!resp.ok || result.success === false) {
                    alert(result.message || '新增學校失敗');
                    return;
                }

                // 新增到下拉選單並選取
                const option = document.createElement('option');
                option.value = result.schoolId;
                option.textContent = result.schoolName;
                schoolSelect.add(option);
                schoolSelect.value = String(result.schoolId);

                // 隱藏表單並清空
                addSchoolForm.style.display = 'none';
                if (newSchoolName) newSchoolName.value = '';
                if (newSchoolType) newSchoolType.value = '';

                alert('學校新增成功！');
            } catch (err) {
                console.error('新增學校發生錯誤:', err);
                alert('新增學校發生錯誤');
            } finally {
                // 恢復按鈕
                submitNewSchool.disabled = false;
                submitNewSchool.innerHTML = '<i class="fas fa-check"></i> 新增';
            }
        });

        // 按 Enter 鍵送出
        if (newSchoolName) {
            newSchoolName.addEventListener('keypress', function(e) {
                if (e.key === 'Enter') {
                    submitNewSchool.click();
                }
            });
        }

        if (newSchoolType) {
            newSchoolType.addEventListener('keypress', function(e) {
                if (e.key === 'Enter') {
                    submitNewSchool.click();
                }
            });
        }
    }
}

// 照片預覽功能（支援新增和編輯模式）
function initPhotoPreview(photoUrl = null) {
    const photoFileInput = document.getElementById('photoFileInput');
    const addPhotoBtn = document.getElementById('addPhotoBtn');
    const photoPreviewContainer = document.getElementById('photoPreviewContainer');
    const photoPlaceholder = document.getElementById('photoPlaceholder');
    const photoLoadingContainer = document.getElementById('photoLoadingContainer');
    const previewImage = document.getElementById('previewImage');

    if (!photoFileInput || !addPhotoBtn || !photoPreviewContainer || !photoPlaceholder || !previewImage) return;

    // 載入已存在的照片（編輯模式）
    if (photoUrl && photoUrl.trim() !== '') {
        if (photoLoadingContainer) {
            photoPlaceholder.classList.add('hidden');
            photoPreviewContainer.classList.add('hidden');
            photoLoadingContainer.classList.remove('hidden');
        }
        
        previewImage.onload = function() {
            if (photoLoadingContainer) {
                photoLoadingContainer.classList.add('hidden');
            }
            photoPlaceholder.classList.add('hidden');
            photoPreviewContainer.classList.remove('hidden');
            if (addPhotoBtn) {
                addPhotoBtn.innerHTML = '<i class="bi bi-pencil me-1"></i>更換照片';
            }
        };
        
        previewImage.onerror = function() {
            console.error('圖片載入失敗:', photoUrl);
            if (photoLoadingContainer) {
                photoLoadingContainer.classList.add('hidden');
            }
            photoPreviewContainer.classList.add('hidden');
            photoPlaceholder.classList.remove('hidden');
        };
        
        previewImage.src = photoUrl;
    }

    // 點擊「新增照片」按鈕時觸發檔案選擇
    addPhotoBtn.addEventListener('click', function() {
        photoFileInput.click();
    });

    // 檔案選擇變更時
    photoFileInput.addEventListener('change', function(e) {
        const file = e.target.files[0];
        if (file) {
            // 驗證檔案類型
            const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
            if (!allowedTypes.includes(file.type)) {
                alert('僅支援 JPG、PNG、GIF 格式的圖片');
                photoFileInput.value = '';
                photoPlaceholder.classList.remove('hidden');
                photoPreviewContainer.classList.add('hidden');
                if (photoLoadingContainer) {
                    photoLoadingContainer.classList.add('hidden');
                }
                return;
            }

            // 驗證檔案大小（5MB）
            if (file.size > 5 * 1024 * 1024) {
                alert('檔案大小不能超過 5MB');
                photoFileInput.value = '';
                photoPlaceholder.classList.remove('hidden');
                photoPreviewContainer.classList.add('hidden');
                if (photoLoadingContainer) {
                    photoLoadingContainer.classList.add('hidden');
                }
                return;
            }

            // 顯示預覽
            const reader = new FileReader();
            reader.onload = function(e) {
                previewImage.src = e.target.result;
                if (photoLoadingContainer) {
                    photoLoadingContainer.classList.add('hidden');
                }
                photoPlaceholder.classList.add('hidden');
                photoPreviewContainer.classList.remove('hidden');
                // 更新按鈕文字
                addPhotoBtn.innerHTML = '<i class="bi bi-pencil me-1"></i>更換照片';
            };
            reader.readAsDataURL(file);
        } else {
            // 如果沒有選擇檔案，恢復原狀態
            if (!photoUrl || photoUrl.trim() === '') {
                photoPlaceholder.classList.remove('hidden');
                photoPreviewContainer.classList.add('hidden');
                if (photoLoadingContainer) {
                    photoLoadingContainer.classList.add('hidden');
                }
                addPhotoBtn.innerHTML = '<i class="bi bi-plus-circle me-1"></i>新增照片';
            }
        }
    });

    // 點擊照片預覽區域也可以觸發檔案選擇
    photoPreviewContainer.addEventListener('click', function() {
        photoFileInput.click();
    });
}

