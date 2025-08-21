// Custom JavaScript for enhanced Swagger UI functionality
// Digital Library API Documentation Enhancements

// Configuration
const CONFIG = {
    apiBaseUrl: window.location.origin,
    demoTokens: {
        admin: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbiIsInJvbGUiOiJhZG1pbiIsImV4cCI6OTk5OTk5OTk5OX0.demo-admin-token',
        librarian: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJsaWJyYXJpYW4iLCJyb2xlIjoibGlicmFyaWFuIiwiZXhwIjo5OTk5OTk5OTk5fQ.demo-librarian-token',
        user: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ1c2VyIiwicm9sZSI6InVzZXIiLCJleHAiOjk5OTk5OTk5OTl9.demo-user-token'
    },
    demoApiKeys: {
        development: 'dev-api-key-12345',
        testing: 'test-api-key-67890',
        production: 'prod-api-key-abcdef'
    }
};

// Add custom functionality when the page loads
window.addEventListener('load', function() {
    setTimeout(function() {
        initializeEnhancements();
    }, 1000);
});

// Initialize all enhancements
function initializeEnhancements() {
    addVersionInfo();
    addCustomButtons();
    enhanceExamples();
    addAuthenticationHelpers();
    addThemeToggle();
    addKeyboardShortcuts();
    addPerformanceMetrics();
}

// Add version information to the UI
function addVersionInfo() {
    const infoSection = document.querySelector('.info');
    if (infoSection && !document.querySelector('.version-info')) {
        const versionInfo = document.createElement('div');
        versionInfo.className = 'version-info';
        versionInfo.innerHTML = `
            <div style="background: #f7f7f7; padding: 15px; margin: 15px 0; border-radius: 5px; border-left: 4px solid #007bff;">
                <h4 style="margin: 0 0 10px 0; color: #007bff;">API Version Information</h4>
                <p style="margin: 5px 0;"><strong>Current Version:</strong> v1.0</p>
                <p style="margin: 5px 0;"><strong>Status:</strong> Stable</p>
                <p style="margin: 5px 0;"><strong>Last Updated:</strong> ${new Date().toLocaleDateString()}</p>
                <p style="margin: 5px 0;"><strong>Breaking Changes:</strong> None</p>
            </div>
        `;
        infoSection.appendChild(versionInfo);
    }
}

// Add enhanced custom action buttons
function addCustomButtons() {
    const topbar = document.querySelector('.topbar');
    if (topbar && !document.querySelector('.custom-actions')) {
        const customActions = document.createElement('div');
        customActions.className = 'custom-actions';
        customActions.style.cssText = 'display: flex; gap: 8px; align-items: center; flex-wrap: wrap;';
        
        // Download OpenAPI spec button
        const downloadBtn = createActionButton('📥 Download Spec', '#28a745', function() {
            downloadOpenApiSpec();
        });
        
        // API Health Check button
        const healthBtn = createActionButton('🏥 Health Check', '#17a2b8', function() {
            checkApiHealth();
        });
        
        // Quick Test button
        const testBtn = createActionButton('🧪 Quick Test', '#ffc107', function() {
            runQuickTest();
        }, 'black');
        
        // Authentication Helper button
        const authBtn = createActionButton('🔐 Auth Helper', '#6f42c1', function() {
            showAuthenticationHelper();
        });
        
        // API Explorer button
        const explorerBtn = createActionButton('🗺️ Explorer', '#fd7e14', function() {
            showApiExplorer();
        });
        
        customActions.appendChild(downloadBtn);
        customActions.appendChild(healthBtn);
        customActions.appendChild(testBtn);
        customActions.appendChild(authBtn);
        customActions.appendChild(explorerBtn);
        
        const topbarWrapper = topbar.querySelector('.topbar-wrapper');
        if (topbarWrapper) {
            topbarWrapper.appendChild(customActions);
        }
    }
}

// Helper function to create action buttons
function createActionButton(text, bgColor, onClick, textColor = 'white') {
    const button = document.createElement('button');
    button.innerHTML = text;
    button.className = 'btn custom-action-btn';
    button.style.cssText = `
        background: ${bgColor}; 
        color: ${textColor}; 
        border: none; 
        padding: 8px 12px; 
        border-radius: 6px; 
        cursor: pointer; 
        font-size: 11px; 
        font-weight: 600;
        transition: all 0.2s ease;
        text-transform: uppercase;
        letter-spacing: 0.5px;
    `;
    button.onclick = onClick;
    
    // Add hover effects
    button.addEventListener('mouseenter', function() {
        this.style.transform = 'translateY(-1px)';
        this.style.boxShadow = '0 4px 8px rgba(0,0,0,0.2)';
    });
    
    button.addEventListener('mouseleave', function() {
        this.style.transform = 'translateY(0)';
        this.style.boxShadow = 'none';
    });
    
    return button;
}

// Enhance examples with better formatting
function enhanceExamples() {
    // Add tooltips to operation IDs
    const operationIds = document.querySelectorAll('.opblock-summary-operation-id');
    operationIds.forEach(function(element) {
        element.title = 'Operation ID: ' + element.textContent;
        element.style.cursor = 'help';
    });
    
    // Add status indicators for different HTTP methods
    const methods = document.querySelectorAll('.opblock .opblock-summary-method');
    methods.forEach(function(method) {
        const methodText = method.textContent.toUpperCase();
        let emoji = '';
        switch(methodText) {
            case 'GET': emoji = '📖'; break;
            case 'POST': emoji = '➕'; break;
            case 'PUT': emoji = '✏️'; break;
            case 'DELETE': emoji = '🗑️'; break;
            case 'PATCH': emoji = '🔧'; break;
        }
        if (emoji) {
            method.innerHTML = emoji + ' ' + method.innerHTML;
        }
    });
}

// Check API health
function checkApiHealth() {
    const healthUrl = window.location.origin + '/api/v1/library/info';
    
    fetch(healthUrl)
        .then(response => response.json())
        .then(data => {
            showNotification('✅ API is healthy! Library has ' + data.totalBooks + ' books and ' + data.totalAuthors + ' authors.', 'success');
        })
        .catch(error => {
            showNotification('❌ API health check failed: ' + error.message, 'error');
        });
}

// Run a quick test of the API
function runQuickTest() {
    const testUrl = window.location.origin + '/api/v1/authors?page=1&pageSize=1';
    
    fetch(testUrl)
        .then(response => response.json())
        .then(data => {
            if (data.items && data.items.length > 0) {
                showNotification('✅ Quick test passed! Found author: ' + data.items[0].fullName, 'success');
            } else {
                showNotification('⚠️ Quick test completed but no authors found.', 'warning');
            }
        })
        .catch(error => {
            showNotification('❌ Quick test failed: ' + error.message, 'error');
        });
}

// Show notification
function showNotification(message, type) {
    // Remove existing notifications
    const existing = document.querySelector('.custom-notification');
    if (existing) {
        existing.remove();
    }
    
    const notification = document.createElement('div');
    notification.className = 'custom-notification';
    
    let bgColor = '#007bff';
    if (type === 'success') bgColor = '#28a745';
    if (type === 'error') bgColor = '#dc3545';
    if (type === 'warning') bgColor = '#ffc107';
    
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: ${bgColor};
        color: ${type === 'warning' ? 'black' : 'white'};
        padding: 15px 20px;
        border-radius: 5px;
        box-shadow: 0 4px 6px rgba(0,0,0,0.1);
        z-index: 10000;
        max-width: 400px;
        font-size: 14px;
        animation: slideIn 0.3s ease-out;
    `;
    
    notification.innerHTML = message + '<span style="margin-left: 15px; cursor: pointer; font-weight: bold;" onclick="this.parentElement.remove()">×</span>';
    
    document.body.appendChild(notification);
    
    // Auto-remove after 5 seconds
    setTimeout(function() {
        if (notification.parentElement) {
            notification.remove();
        }
    }, 5000);
}

// Add CSS animations
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    .custom-actions .btn:hover {
        opacity: 0.8;
        transform: translateY(-1px);
        transition: all 0.2s ease;
    }
    
    .version-info {
        animation: fadeIn 0.5s ease-in;
    }
    
    @keyframes fadeIn {
        from { opacity: 0; }
        to { opacity: 1; }
    }
`;
document.head.appendChild(style);
// Aut
hentication Helper Functions
function addAuthenticationHelpers() {
    // Add demo credentials to the page
    setTimeout(function() {
        const authSection = document.querySelector('.auth-wrapper');
        if (authSection && !document.querySelector('.demo-credentials')) {
            const demoSection = document.createElement('div');
            demoSection.className = 'demo-credentials';
            demoSection.innerHTML = `
                <div style="background: #e3f2fd; border: 2px solid #2196f3; border-radius: 8px; padding: 15px; margin: 15px 0;">
                    <h4 style="color: #1976d2; margin: 0 0 10px 0;">🎭 Demo Authentication Credentials</h4>
                    <p style="margin: 5px 0; font-size: 14px; color: #555;">Use these demo credentials to test the API:</p>
                    <div style="display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 10px; margin: 10px 0;">
                        <div style="background: white; padding: 10px; border-radius: 4px; border-left: 4px solid #4caf50;">
                            <strong>👤 User Role</strong><br>
                            <small>API Key: <code>dev-api-key-12345</code></small><br>
                            <small>JWT: <code>demo-user-token</code></small>
                        </div>
                        <div style="background: white; padding: 10px; border-radius: 4px; border-left: 4px solid #ff9800;">
                            <strong>📚 Librarian Role</strong><br>
                            <small>API Key: <code>test-api-key-67890</code></small><br>
                            <small>JWT: <code>demo-librarian-token</code></small>
                        </div>
                        <div style="background: white; padding: 10px; border-radius: 4px; border-left: 4px solid #f44336;">
                            <strong>👑 Admin Role</strong><br>
                            <small>API Key: <code>prod-api-key-abcdef</code></small><br>
                            <small>JWT: <code>demo-admin-token</code></small>
                        </div>
                    </div>
                    <div style="margin-top: 15px;">
                        <button onclick="fillDemoCredentials('user')" style="background: #4caf50; color: white; border: none; padding: 6px 12px; border-radius: 4px; margin: 2px; cursor: pointer; font-size: 12px;">Fill User Credentials</button>
                        <button onclick="fillDemoCredentials('librarian')" style="background: #ff9800; color: white; border: none; padding: 6px 12px; border-radius: 4px; margin: 2px; cursor: pointer; font-size: 12px;">Fill Librarian Credentials</button>
                        <button onclick="fillDemoCredentials('admin')" style="background: #f44336; color: white; border: none; padding: 6px 12px; border-radius: 4px; margin: 2px; cursor: pointer; font-size: 12px;">Fill Admin Credentials</button>
                    </div>
                </div>
            `;
            authSection.appendChild(demoSection);
        }
    }, 2000);
}

// Fill demo credentials function
window.fillDemoCredentials = function(role) {
    // Fill JWT Bearer token
    const bearerInput = document.querySelector('input[placeholder*="Bearer"], input[name*="Authorization"]');
    if (bearerInput) {
        bearerInput.value = CONFIG.demoTokens[role];
        bearerInput.dispatchEvent(new Event('input', { bubbles: true }));
    }
    
    // Fill API Key
    const apiKeyInput = document.querySelector('input[placeholder*="API"], input[name*="X-API-Key"]');
    if (apiKeyInput) {
        const apiKeyMap = { user: 'development', librarian: 'testing', admin: 'production' };
        apiKeyInput.value = CONFIG.demoApiKeys[apiKeyMap[role]];
        apiKeyInput.dispatchEvent(new Event('input', { bubbles: true }));
    }
    
    showNotification(`✅ ${role.charAt(0).toUpperCase() + role.slice(1)} credentials filled successfully!`, 'success');
};

// Show Authentication Helper Modal
function showAuthenticationHelper() {
    const modal = createModal('Authentication Helper', `
        <div style="max-width: 600px;">
            <h3>🔐 Authentication Methods</h3>
            <div style="margin: 20px 0;">
                <h4>1. Bearer Token (JWT)</h4>
                <p>Use JWT tokens for user authentication. The token should be prefixed with "Bearer ".</p>
                <div style="background: #f8f9fa; padding: 10px; border-radius: 4px; font-family: monospace; font-size: 12px; margin: 10px 0;">
                    Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
                </div>
                
                <h4>2. API Key</h4>
                <p>Use API keys for service-to-service authentication via the X-API-Key header.</p>
                <div style="background: #f8f9fa; padding: 10px; border-radius: 4px; font-family: monospace; font-size: 12px; margin: 10px 0;">
                    X-API-Key: your-api-key-here
                </div>
                
                <h4>3. Basic Authentication</h4>
                <p>Use username and password for basic HTTP authentication.</p>
                <div style="background: #f8f9fa; padding: 10px; border-radius: 4px; font-family: monospace; font-size: 12px; margin: 10px 0;">
                    Authorization: Basic dXNlcm5hbWU6cGFzc3dvcmQ=
                </div>
                
                <h4>4. OAuth2 (Simulated)</h4>
                <p>OAuth2 flow simulation for demonstration purposes.</p>
            </div>
            
            <div style="background: #fff3cd; padding: 15px; border-radius: 6px; margin: 15px 0;">
                <strong>⚠️ Note:</strong> This is a demonstration API. All authentication is simulated and no real security is enforced.
            </div>
            
            <div style="text-align: center; margin-top: 20px;">
                <button onclick="closeModal()" style="background: #6c757d; color: white; border: none; padding: 10px 20px; border-radius: 4px; cursor: pointer;">Close</button>
            </div>
        </div>
    `);
}

// Download OpenAPI Spec with options
function downloadOpenApiSpec() {
    const modal = createModal('Download OpenAPI Specification', `
        <div style="max-width: 500px;">
            <h3>📥 Download Options</h3>
            <div style="margin: 20px 0;">
                <button onclick="downloadSpec('json')" style="background: #007bff; color: white; border: none; padding: 12px 20px; border-radius: 4px; cursor: pointer; margin: 5px; width: 100%;">
                    📄 Download JSON Format
                </button>
                <button onclick="downloadSpec('yaml')" style="background: #28a745; color: white; border: none; padding: 12px 20px; border-radius: 4px; cursor: pointer; margin: 5px; width: 100%;">
                    📝 Download YAML Format
                </button>
                <button onclick="viewSpec()" style="background: #17a2b8; color: white; border: none; padding: 12px 20px; border-radius: 4px; cursor: pointer; margin: 5px; width: 100%;">
                    👁️ View in New Tab
                </button>
            </div>
            <div style="text-align: center; margin-top: 20px;">
                <button onclick="closeModal()" style="background: #6c757d; color: white; border: none; padding: 10px 20px; border-radius: 4px; cursor: pointer;">Close</button>
            </div>
        </div>
    `);
}

window.downloadSpec = function(format) {
    const baseUrl = CONFIG.apiBaseUrl;
    let url = `${baseUrl}/swagger/v1/swagger.json`;
    
    if (format === 'yaml') {
        // Convert JSON to YAML (simplified)
        fetch(url)
            .then(response => response.json())
            .then(data => {
                const yamlContent = JSON.stringify(data, null, 2); // Simplified YAML conversion
                const blob = new Blob([yamlContent], { type: 'application/x-yaml' });
                const downloadUrl = URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = downloadUrl;
                a.download = 'digital-library-api.yaml';
                a.click();
                URL.revokeObjectURL(downloadUrl);
            });
    } else {
        const a = document.createElement('a');
        a.href = url;
        a.download = 'digital-library-api.json';
        a.click();
    }
    
    closeModal();
    showNotification(`📥 OpenAPI spec downloaded in ${format.toUpperCase()} format`, 'success');
};

window.viewSpec = function() {
    const url = `${CONFIG.apiBaseUrl}/swagger/v1/swagger.json`;
    window.open(url, '_blank');
    closeModal();
};

// API Explorer
function showApiExplorer() {
    const modal = createModal('API Explorer', `
        <div style="max-width: 700px;">
            <h3>🗺️ API Quick Explorer</h3>
            <div style="display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 15px; margin: 20px 0;">
                <div style="background: #e8f5e8; padding: 15px; border-radius: 8px; border-left: 4px solid #28a745;">
                    <h4 style="margin: 0 0 10px 0; color: #155724;">📚 Books</h4>
                    <p style="font-size: 14px; margin: 5px 0;">Manage library books</p>
                    <button onclick="scrollToTag('Books')" style="background: #28a745; color: white; border: none; padding: 6px 12px; border-radius: 4px; cursor: pointer; font-size: 12px;">View Endpoints</button>
                </div>
                <div style="background: #e3f2fd; padding: 15px; border-radius: 8px; border-left: 4px solid #2196f3;">
                    <h4 style="margin: 0 0 10px 0; color: #0d47a1;">👤 Authors</h4>
                    <p style="font-size: 14px; margin: 5px 0;">Manage book authors</p>
                    <button onclick="scrollToTag('Authors')" style="background: #2196f3; color: white; border: none; padding: 6px 12px; border-radius: 4px; cursor: pointer; font-size: 12px;">View Endpoints</button>
                </div>
                <div style="background: #fff3e0; padding: 15px; border-radius: 8px; border-left: 4px solid #ff9800;">
                    <h4 style="margin: 0 0 10px 0; color: #e65100;">📋 Loans</h4>
                    <p style="font-size: 14px; margin: 5px 0;">Manage book loans</p>
                    <button onclick="scrollToTag('Loans')" style="background: #ff9800; color: white; border: none; padding: 6px 12px; border-radius: 4px; cursor: pointer; font-size: 12px;">View Endpoints</button>
                </div>
                <div style="background: #fce4ec; padding: 15px; border-radius: 8px; border-left: 4px solid #e91e63;">
                    <h4 style="margin: 0 0 10px 0; color: #880e4f;">ℹ️ Library Info</h4>
                    <p style="font-size: 14px; margin: 5px 0;">Library statistics</p>
                    <button onclick="scrollToTag('Library')" style="background: #e91e63; color: white; border: none; padding: 6px 12px; border-radius: 4px; cursor: pointer; font-size: 12px;">View Endpoints</button>
                </div>
            </div>
            <div style="text-align: center; margin-top: 20px;">
                <button onclick="closeModal()" style="background: #6c757d; color: white; border: none; padding: 10px 20px; border-radius: 4px; cursor: pointer;">Close</button>
            </div>
        </div>
    `);
}

window.scrollToTag = function(tagName) {
    closeModal();
    setTimeout(() => {
        const tagElement = Array.from(document.querySelectorAll('.opblock-tag')).find(el => 
            el.textContent.includes(tagName)
        );
        if (tagElement) {
            tagElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
            tagElement.style.background = '#fff3cd';
            setTimeout(() => {
                tagElement.style.background = '';
            }, 2000);
        }
    }, 100);
};

// Theme Toggle
function addThemeToggle() {
    const topbar = document.querySelector('.topbar .topbar-wrapper');
    if (topbar && !document.querySelector('.theme-toggle')) {
        const themeToggle = document.createElement('button');
        themeToggle.className = 'theme-toggle';
        themeToggle.innerHTML = '🌙';
        themeToggle.style.cssText = `
            background: rgba(255,255,255,0.2);
            color: white;
            border: none;
            padding: 8px 12px;
            border-radius: 50%;
            cursor: pointer;
            font-size: 16px;
            margin-left: 15px;
            transition: all 0.3s ease;
        `;
        
        themeToggle.onclick = function() {
            toggleTheme();
        };
        
        topbar.appendChild(themeToggle);
    }
}

function toggleTheme() {
    const body = document.body;
    const isDark = body.classList.contains('dark-theme');
    
    if (isDark) {
        body.classList.remove('dark-theme');
        document.querySelector('.theme-toggle').innerHTML = '🌙';
        localStorage.setItem('swagger-theme', 'light');
    } else {
        body.classList.add('dark-theme');
        document.querySelector('.theme-toggle').innerHTML = '☀️';
        localStorage.setItem('swagger-theme', 'dark');
    }
}

// Keyboard Shortcuts
function addKeyboardShortcuts() {
    document.addEventListener('keydown', function(e) {
        // Ctrl/Cmd + K: Focus search
        if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
            e.preventDefault();
            const searchInput = document.querySelector('.filter input');
            if (searchInput) {
                searchInput.focus();
                showNotification('🔍 Search focused! Type to filter operations.', 'info');
            }
        }
        
        // Ctrl/Cmd + D: Download spec
        if ((e.ctrlKey || e.metaKey) && e.key === 'd') {
            e.preventDefault();
            downloadOpenApiSpec();
        }
        
        // Ctrl/Cmd + H: Show help
        if ((e.ctrlKey || e.metaKey) && e.key === 'h') {
            e.preventDefault();
            showKeyboardHelp();
        }
    });
}

function showKeyboardHelp() {
    const modal = createModal('Keyboard Shortcuts', `
        <div style="max-width: 500px;">
            <h3>⌨️ Keyboard Shortcuts</h3>
            <div style="margin: 20px 0;">
                <div style="display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #eee;">
                    <span><kbd>Ctrl/Cmd + K</kbd></span>
                    <span>Focus search filter</span>
                </div>
                <div style="display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #eee;">
                    <span><kbd>Ctrl/Cmd + D</kbd></span>
                    <span>Download OpenAPI spec</span>
                </div>
                <div style="display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #eee;">
                    <span><kbd>Ctrl/Cmd + H</kbd></span>
                    <span>Show this help</span>
                </div>
                <div style="display: flex; justify-content: space-between; padding: 8px 0;">
                    <span><kbd>Esc</kbd></span>
                    <span>Close modals</span>
                </div>
            </div>
            <div style="text-align: center; margin-top: 20px;">
                <button onclick="closeModal()" style="background: #6c757d; color: white; border: none; padding: 10px 20px; border-radius: 4px; cursor: pointer;">Close</button>
            </div>
        </div>
    `);
}

// Performance Metrics
function addPerformanceMetrics() {
    let requestCount = 0;
    let totalResponseTime = 0;
    
    // Monitor fetch requests
    const originalFetch = window.fetch;
    window.fetch = function(...args) {
        const startTime = performance.now();
        requestCount++;
        
        return originalFetch.apply(this, args).then(response => {
            const endTime = performance.now();
            const responseTime = endTime - startTime;
            totalResponseTime += responseTime;
            
            updatePerformanceDisplay(requestCount, totalResponseTime / requestCount);
            return response;
        });
    };
}

function updatePerformanceDisplay(count, avgTime) {
    let perfDisplay = document.querySelector('.performance-metrics');
    if (!perfDisplay) {
        perfDisplay = document.createElement('div');
        perfDisplay.className = 'performance-metrics';
        perfDisplay.style.cssText = `
            position: fixed;
            bottom: 20px;
            right: 20px;
            background: rgba(0,0,0,0.8);
            color: white;
            padding: 10px 15px;
            border-radius: 8px;
            font-size: 12px;
            z-index: 1000;
            font-family: monospace;
        `;
        document.body.appendChild(perfDisplay);
    }
    
    perfDisplay.innerHTML = `
        📊 Requests: ${count}<br>
        ⏱️ Avg: ${avgTime.toFixed(0)}ms
    `;
}

// Modal Helper Functions
function createModal(title, content) {
    // Remove existing modal
    const existingModal = document.querySelector('.custom-modal');
    if (existingModal) {
        existingModal.remove();
    }
    
    const modal = document.createElement('div');
    modal.className = 'custom-modal';
    modal.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background: rgba(0,0,0,0.5);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 10000;
    `;
    
    modal.innerHTML = `
        <div style="background: white; border-radius: 12px; padding: 30px; max-width: 90vw; max-height: 90vh; overflow-y: auto; box-shadow: 0 10px 30px rgba(0,0,0,0.3);">
            <h2 style="margin: 0 0 20px 0; color: #2c3e50; display: flex; align-items: center; gap: 10px;">
                ${title}
                <button onclick="closeModal()" style="background: none; border: none; font-size: 24px; cursor: pointer; margin-left: auto; color: #999;">×</button>
            </h2>
            ${content}
        </div>
    `;
    
    document.body.appendChild(modal);
    
    // Close on escape key
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape') {
            closeModal();
        }
    });
    
    // Close on backdrop click
    modal.addEventListener('click', function(e) {
        if (e.target === modal) {
            closeModal();
        }
    });
    
    return modal;
}

window.closeModal = function() {
    const modal = document.querySelector('.custom-modal');
    if (modal) {
        modal.remove();
    }
};