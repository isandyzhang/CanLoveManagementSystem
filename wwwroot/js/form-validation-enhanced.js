/**
 * 增強的表單驗證功能 - 實作即時驗證（欄位失去焦點時驗證）
 */

(function() {
    'use strict';

    /**
     * 驗證單一欄位
     */
    function validateField(field) {
        // 移除之前的驗證狀態
        field.classList.remove('is-valid', 'is-invalid');
        
        // 如果欄位是空的且不是必填，跳過驗證
        if (!field.required && !field.value.trim()) {
            return true;
        }

        // 執行 HTML5 驗證
        if (field.checkValidity()) {
            field.classList.add('is-valid');
            removeFieldError(field);
            return true;
        } else {
            field.classList.add('is-invalid');
            showFieldError(field);
            return false;
        }
    }

    /**
     * 顯示欄位錯誤訊息
     */
    function showFieldError(field) {
        // 移除現有的錯誤訊息
        removeFieldError(field);

        // 取得驗證訊息
        let message = field.validationMessage;
        
        // 如果沒有自訂訊息，使用預設訊息
        if (!message || message === '') {
            if (field.required) {
                message = '此欄位為必填';
            } else if (field.type === 'email') {
                message = '請輸入有效的電子郵件格式';
            } else if (field.type === 'tel') {
                message = '請輸入有效的電話號碼格式';
            } else if (field.type === 'url') {
                message = '請輸入有效的網址格式';
            } else if (field.hasAttribute('pattern')) {
                message = '格式不正確';
            } else {
                message = '請檢查此欄位';
            }
        }

        // 建立錯誤訊息元素
        const errorDiv = document.createElement('div');
        errorDiv.className = 'invalid-feedback';
        errorDiv.id = `${field.name || field.id}_error`;
        errorDiv.textContent = message;

        // 插入錯誤訊息（在欄位後面）
        field.parentNode.insertBefore(errorDiv, field.nextSibling);
    }

    /**
     * 移除欄位錯誤訊息
     */
    function removeFieldError(field) {
        const errorDiv = field.parentNode.querySelector(`#${field.name || field.id}_error`);
        if (errorDiv) {
            errorDiv.remove();
        }
    }

    /**
     * 初始化表單即時驗證
     */
    function initRealTimeValidation(form) {
        // 取得所有需要驗證的欄位
        const fields = form.querySelectorAll('input, select, textarea');
        
        fields.forEach(field => {
            // 跳過隱藏欄位和按鈕
            if (field.type === 'hidden' || field.type === 'submit' || field.type === 'button') {
                return;
            }

            // 失去焦點時驗證
            field.addEventListener('blur', function() {
                validateField(field);
            });

            // 輸入時清除錯誤狀態（但保留驗證狀態）
            field.addEventListener('input', function() {
                if (field.classList.contains('is-invalid')) {
                    // 如果欄位現在有效，移除錯誤狀態
                    if (field.checkValidity()) {
                        field.classList.remove('is-invalid');
                        field.classList.add('is-valid');
                        removeFieldError(field);
                    }
                }
            });
        });
    }

    /**
     * 驗證整個表單
     */
    function validateForm(form) {
        const fields = form.querySelectorAll('input, select, textarea');
        let isValid = true;

        fields.forEach(field => {
            // 跳過隱藏欄位和按鈕
            if (field.type === 'hidden' || field.type === 'submit' || field.type === 'button') {
                return;
            }

            if (!validateField(field)) {
                isValid = false;
            }
        });

        return isValid;
    }

    /**
     * 初始化所有表單
     */
    function init() {
        const forms = document.querySelectorAll('form.needs-validation, form.case-detail-form');
        
        forms.forEach(form => {
            // 初始化即時驗證
            initRealTimeValidation(form);

            // 表單提交時驗證
            form.addEventListener('submit', function(e) {
                if (!validateForm(form)) {
                    e.preventDefault();
                    e.stopPropagation();

                    // 聚焦到第一個無效欄位
                    const firstInvalid = form.querySelector(':invalid');
                    if (firstInvalid) {
                        firstInvalid.focus();
                        firstInvalid.scrollIntoView({ behavior: 'smooth', block: 'center' });
                    }

                    // 顯示驗證錯誤通知
                    if (typeof window.FormNotification !== 'undefined' && window.FormNotification.showError) {
                        window.FormNotification.showError('請檢查表單中的錯誤欄位');
                    }
                } else {
                    form.classList.add('was-validated');
                }
            });
        });
    }

    // DOM 載入後初始化
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    // 提供全域函數
    window.FormValidationEnhanced = {
        validateField,
        validateForm,
        initRealTimeValidation
    };
})();
