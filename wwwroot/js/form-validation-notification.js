/**
 * 表單驗證通知功能
 * 使用專案的 AlertMessage 風格顯示驗證錯誤
 */

/**
 * 顯示驗證錯誤通知
 * @param {string} message - 錯誤訊息
 */
function showValidationError(message) {
    // 移除現有的驗證錯誤通知
    const existingAlert = document.querySelector('.validation-error-alert');
    if (existingAlert) {
        existingAlert.remove();
    }

    // 創建通知元素（使用專案的 AlertMessage 風格）
    const alertDiv = document.createElement('div');
    alertDiv.className = 'validation-error-alert alert-message mb-3 rounded-md border bg-red-50 dark:bg-red-900/20 border-red-200 dark:border-red-800 shadow-sm';
    alertDiv.setAttribute('role', 'alert');
    alertDiv.innerHTML = `
        <div class="flex items-center justify-between px-4 py-3">
            <div class="flex items-center gap-2 text-red-600 dark:text-red-400">
                <i class="bi bi-exclamation-triangle text-lg"></i>
                <span class="text-red-800 dark:text-red-200 font-medium">${message}</span>
            </div>
            <button type="button" 
                    aria-label="關閉" 
                    class="p-2 rounded-md hover:bg-black/5 dark:hover:bg-white/10 text-red-800 dark:text-red-200 transition-colors"
                    onclick="this.closest('.validation-error-alert').remove()">
                <i class="bi bi-x text-lg"></i>
            </button>
        </div>
    `;

    // 找到表單容器或 main 元素來插入通知
    const form = document.querySelector('form.needs-validation');
    if (form) {
        // 插入到表單之前
        const container = form.closest('.w-full') || form.parentElement;
        if (container) {
            container.insertBefore(alertDiv, form);
        } else {
            // 如果找不到容器，插入到表單之前
            form.parentNode.insertBefore(alertDiv, form);
        }
    } else {
        // 如果找不到表單，插入到 main 元素的最前面
        const main = document.querySelector('main[role="main"]');
        if (main) {
            const firstChild = main.firstElementChild;
            if (firstChild) {
                main.insertBefore(alertDiv, firstChild);
            } else {
                main.appendChild(alertDiv);
            }
        }
    }

    // 滾動到通知位置
    alertDiv.scrollIntoView({ behavior: 'smooth', block: 'nearest' });

    // 5秒後自動關閉
    setTimeout(function() {
        if (alertDiv.parentNode) {
            alertDiv.style.transition = 'opacity 0.3s ease-out';
            alertDiv.style.opacity = '0';
            setTimeout(function() {
                if (alertDiv.parentNode) {
                    alertDiv.remove();
                }
            }, 300);
        }
    }, 5000);
}

/**
 * 初始化表單驗證並顯示通知
 * 使用方式：在表單上加入 class="needs-validation"
 */
function initFormValidation() {
    'use strict';
    
    window.addEventListener('load', function() {
        var forms = document.getElementsByClassName('needs-validation');
        var validation = Array.prototype.filter.call(forms, function(form) {
            form.addEventListener('submit', function(event) {
                if (form.checkValidity() === false) {
                    event.preventDefault();
                    event.stopPropagation();
                    
                    // 顯示驗證錯誤通知
                    showValidationError('請填寫所有必填欄位後再提交');
                    
                    // 聚焦到第一個無效欄位
                    const firstInvalid = form.querySelector(':invalid');
                    if (firstInvalid) {
                        firstInvalid.focus();
                        firstInvalid.scrollIntoView({ behavior: 'smooth', block: 'center' });
                    }
                }
                form.classList.add('was-validated');
            }, false);
        });
    }, false);
}

// DOM 載入後初始化
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initFormValidation);
} else {
    initFormValidation();
}

