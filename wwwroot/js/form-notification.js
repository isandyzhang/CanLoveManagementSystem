/**
 * 表單通知功能 - 改進表單提交後的成功/錯誤訊息顯示
 */

(function() {
    'use strict';

    /**
     * 顯示 Toast 通知
     */
    function showToast(message, type = 'info') {
        // 移除現有的 toast
        const existingToasts = document.querySelectorAll('.toast-notification');
        existingToasts.forEach(toast => toast.remove());

        // 建立 toast 元素
        const toast = document.createElement('div');
        toast.className = `toast-notification fixed top-4 right-4 z-50 p-4 rounded-lg shadow-lg flex items-center gap-3 min-w-[300px] max-w-[500px] animate-slide-in`;
        
        // 根據類型設定樣式
        const typeStyles = {
            success: 'bg-green-500 text-white',
            error: 'bg-red-500 text-white',
            warning: 'bg-yellow-500 text-white',
            info: 'bg-blue-500 text-white'
        };
        
        const icons = {
            success: 'bi-check-circle',
            error: 'bi-x-circle',
            warning: 'bi-exclamation-triangle',
            info: 'bi-info-circle'
        };
        
        toast.className += ` ${typeStyles[type] || typeStyles.info}`;
        
        toast.innerHTML = `
            <i class="bi ${icons[type] || icons.info} text-2xl"></i>
            <div class="flex-1">
                <p class="font-medium">${escapeHtml(message)}</p>
            </div>
            <button class="toast-close ml-2 text-white hover:text-gray-200" onclick="this.parentElement.remove()">
                <i class="bi bi-x-lg"></i>
            </button>
        `;
        
        document.body.appendChild(toast);
        
        // 自動移除（5秒後）
        setTimeout(() => {
            toast.style.opacity = '0';
            toast.style.transition = 'opacity 0.3s';
            setTimeout(() => toast.remove(), 300);
        }, 5000);
    }

    /**
     * 顯示成功訊息
     */
    function showSuccess(message) {
        showToast(message, 'success');
    }

    /**
     * 顯示錯誤訊息
     */
    function showError(message) {
        showToast(message, 'error');
    }

    /**
     * 顯示警告訊息
     */
    function showWarning(message) {
        showToast(message, 'warning');
    }

    /**
     * 顯示資訊訊息
     */
    function showInfo(message) {
        showToast(message, 'info');
    }

    /**
     * HTML 轉義
     */
    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    /**
     * 檢查 TempData 訊息並顯示
     */
    function checkTempDataMessages() {
        // 檢查是否有成功訊息
        const successMessage = document.querySelector('[data-tempdata-success]');
        if (successMessage) {
            showSuccess(successMessage.textContent.trim());
            successMessage.remove();
        }

        // 檢查是否有錯誤訊息
        const errorMessage = document.querySelector('[data-tempdata-error]');
        if (errorMessage) {
            showError(errorMessage.textContent.trim());
            errorMessage.remove();
        }

        // 檢查是否有警告訊息
        const warningMessage = document.querySelector('[data-tempdata-warning]');
        if (warningMessage) {
            showWarning(warningMessage.textContent.trim());
            warningMessage.remove();
        }
    }

    /**
     * 顯示載入動畫
     */
    function showLoading(message = '載入中...') {
        const loading = document.createElement('div');
        loading.id = 'formLoadingOverlay';
        loading.className = 'fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center';
        loading.innerHTML = `
            <div class="bg-white dark:bg-slate-800 rounded-lg p-6 flex flex-col items-center gap-4">
                <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
                <p class="text-slate-700 dark:text-slate-300">${escapeHtml(message)}</p>
            </div>
        `;
        document.body.appendChild(loading);
    }

    /**
     * 隱藏載入動畫
     */
    function hideLoading() {
        const loading = document.getElementById('formLoadingOverlay');
        if (loading) {
            loading.remove();
        }
    }

    // DOM 載入後檢查訊息
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', checkTempDataMessages);
    } else {
        checkTempDataMessages();
    }

    // 提供全域函數
    window.FormNotification = {
        showSuccess,
        showError,
        showWarning,
        showInfo,
        showLoading,
        hideLoading
    };
})();
