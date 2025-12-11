/**
 * 個案開案紀錄表自動儲存草稿功能
 * 使用 localStorage 儲存表單資料，並在頁面載入時自動恢復
 */

(function() {
    'use strict';

    // 儲存鍵值前綴
    const STORAGE_PREFIX = 'caseOpening_draft_';
    
    // Debounce 延遲時間（毫秒）
    const SAVE_DELAY = 2000; // 2秒後儲存

    /**
     * 取得儲存鍵值
     */
    function getStorageKey(caseId, step) {
        return `${STORAGE_PREFIX}${caseId}_${step}`;
    }

    /**
     * Debounce 函數
     */
    function debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    /**
     * 儲存表單資料到 localStorage
     */
    function saveDraft(form, caseId, step) {
        try {
            const formData = new FormData(form);
            const data = {};
            
            // 收集所有表單欄位值
            for (const [key, value] of formData.entries()) {
                // 跳過防偽令牌和提交按鈕
                if (key === '__RequestVerificationToken' || key.includes('Submit')) {
                    continue;
                }
                data[key] = value;
            }
            
            // 收集 checkbox 和 radio 的狀態
            form.querySelectorAll('input[type="checkbox"], input[type="radio"]').forEach(input => {
                if (input.checked) {
                    data[input.name] = input.value;
                }
            });
            
            // 儲存到 localStorage
            const storageKey = getStorageKey(caseId, step);
            localStorage.setItem(storageKey, JSON.stringify({
                data: data,
                timestamp: new Date().toISOString()
            }));
            
            // 顯示自動儲存提示（可選）
            showAutoSaveIndicator();
        } catch (error) {
            console.warn('自動儲存草稿失敗:', error);
        }
    }

    /**
     * 從 localStorage 載入草稿
     */
    function loadDraft(form, caseId, step) {
        try {
            const storageKey = getStorageKey(caseId, step);
            const saved = localStorage.getItem(storageKey);
            
            if (!saved) {
                return false;
            }
            
            const draft = JSON.parse(saved);
            const data = draft.data;
            
            // 恢復表單資料
            Object.keys(data).forEach(key => {
                const input = form.querySelector(`[name="${key}"]`);
                if (input) {
                    if (input.type === 'checkbox' || input.type === 'radio') {
                        if (input.value === data[key]) {
                            input.checked = true;
                        }
                    } else {
                        input.value = data[key];
                    }
                }
            });
            
            // 觸發 change 事件以更新相關 UI
            form.querySelectorAll('input, select, textarea').forEach(input => {
                if (input.value) {
                    input.dispatchEvent(new Event('change', { bubbles: true }));
                }
            });
            
            return true;
        } catch (error) {
            console.warn('載入草稿失敗:', error);
            return false;
        }
    }

    /**
     * 清除草稿
     */
    function clearDraft(caseId, step) {
        try {
            const storageKey = getStorageKey(caseId, step);
            localStorage.removeItem(storageKey);
        } catch (error) {
            console.warn('清除草稿失敗:', error);
        }
    }

    /**
     * 顯示自動儲存提示
     */
    function showAutoSaveIndicator() {
        // 檢查是否已有提示元素
        let indicator = document.getElementById('autoSaveIndicator');
        
        if (!indicator) {
            // 建立提示元素
            indicator = document.createElement('div');
            indicator.id = 'autoSaveIndicator';
            indicator.className = 'fixed bottom-4 right-4 bg-green-500 text-white px-4 py-2 rounded-lg shadow-lg z-50 flex items-center gap-2';
            indicator.innerHTML = '<i class="bi bi-check-circle"></i> <span>草稿已自動儲存</span>';
            document.body.appendChild(indicator);
        }
        
        // 顯示提示
        indicator.style.display = 'flex';
        
        // 3秒後隱藏
        setTimeout(() => {
            if (indicator) {
                indicator.style.display = 'none';
            }
        }, 3000);
    }

    /**
     * 初始化自動儲存功能
     */
    function initAutoSave() {
        // 尋找所有開案表單
        const forms = document.querySelectorAll('form.prevent-double-submit, form.case-detail-form');
        
        forms.forEach(form => {
            // 取得 caseId 和 step
            const caseIdInput = form.querySelector('input[name="CaseId"]');
            const step = getCurrentStep();
            
            if (!caseIdInput || !caseIdInput.value) {
                return;
            }
            
            const caseId = caseIdInput.value;
            
            // 頁面載入時嘗試恢復草稿
            if (step) {
                const hasDraft = loadDraft(form, caseId, step);
                if (hasDraft) {
                    // 顯示恢復草稿提示
                    const restoreMessage = document.createElement('div');
                    restoreMessage.className = 'alert alert-info mb-3';
                    restoreMessage.innerHTML = '<i class="bi bi-info-circle me-2"></i>已恢復未完成的草稿，您可以繼續填寫。';
                    form.insertBefore(restoreMessage, form.firstChild);
                }
            }
            
            // 建立 debounced 儲存函數
            const debouncedSave = debounce(() => {
                saveDraft(form, caseId, step);
            }, SAVE_DELAY);
            
            // 監聽表單欄位變更
            form.addEventListener('input', debouncedSave);
            form.addEventListener('change', debouncedSave);
            
            // 表單成功提交後清除草稿
            form.addEventListener('submit', function(e) {
                // 延遲清除，確保表單提交成功
                setTimeout(() => {
                    clearDraft(caseId, step);
                }, 1000);
            });
        });
    }

    /**
     * 取得當前步驟
     */
    function getCurrentStep() {
        // 從 URL 或表單中取得步驟資訊
        const url = window.location.pathname;
        const stepMatch = url.match(/\/(Step\d+|CaseDetail|SocialWorkerContent|EconomicStatus|HealthStatus|AcademicPerformance|EmotionalEvaluation|FinalAssessment)/);
        
        if (stepMatch) {
            const stepName = stepMatch[1];
            // 轉換為步驟編號
            const stepMap = {
                'SelectCase': '0',
                'CaseDetail': '1',
                'SocialWorkerContent': '2',
                'EconomicStatus': '3',
                'HealthStatus': '4',
                'AcademicPerformance': '5',
                'EmotionalEvaluation': '6',
                'FinalAssessment': '7'
            };
            
            // 處理 Step0, Step1 等格式
            if (stepName.startsWith('Step')) {
                return stepName.replace('Step', '');
            }
            
            return stepMap[stepName] || null;
        }
        
        return null;
    }

    // DOM 載入後初始化
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initAutoSave);
    } else {
        initAutoSave();
    }

    // 提供全域函數供外部使用
    window.CaseOpeningDraft = {
        save: saveDraft,
        load: loadDraft,
        clear: clearDraft
    };
})();
