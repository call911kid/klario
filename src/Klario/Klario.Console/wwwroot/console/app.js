(() => {
    
    const STORAGE_KEY = 'klario_secret';
    const URL_STORAGE_KEY = 'klario_api_url';

    const ICONS = {
        eye: `<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path><circle cx="12" cy="12" r="3"></circle></svg>`,
        eyeOff: `<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"></path><line x1="1" y1="1" x2="23" y2="23"></line></svg>`
    };

    function getSavedSecret() {
        return sessionStorage.getItem(STORAGE_KEY) || localStorage.getItem(STORAGE_KEY) || '';
    }

    function saveSecret(secret, persistent) {
        if (persistent) {
            localStorage.setItem(STORAGE_KEY, secret);
        } else {
            sessionStorage.setItem(STORAGE_KEY, secret);
        }
    }

    function clearSecret() {
        sessionStorage.removeItem(STORAGE_KEY);
        localStorage.removeItem(STORAGE_KEY);
    }

    const state = {
        profiles: [],
        selectedId: null,
        filter: '',
        targetJobTitles: [],
        targetLocations: []
    };

    const gateView = document.getElementById('gateView');
    const gateForm = document.getElementById('gateForm');
    const gateKeyInput = document.getElementById('gateKeyInput');
    const toggleGateKey = document.getElementById('toggleGateKey');
    const gateRemember = document.getElementById('gateRemember');

    const dashboardApp = document.getElementById('dashboardApp');
    const apiUrlInput = document.getElementById('apiUrl');
    const btnLock = document.getElementById('btnLock');

    const navProfiles = document.getElementById('navProfiles');
    const navBlacklist = document.getElementById('navBlacklist');
    const viewProfiles = document.getElementById('viewProfiles');
    const viewBlacklist = document.getElementById('viewBlacklist');

    const profileSearch = document.getElementById('profileSearch');
    const btnNewProfile = document.getElementById('btnNewProfile');
    const profileCount = document.getElementById('profileCount');
    const sidebarProfiles = document.getElementById('sidebarProfiles');

    const formHeading = document.getElementById('formHeading');
    const btnDeleteProfile = document.getElementById('btnDeleteProfile');

    const form = document.getElementById('profileForm');
    const editIdInput = document.getElementById('editId');
    const nameInput = document.getElementById('profileName');
    const descInput = document.getElementById('profileDesc');
    const activeRow = document.getElementById('activeRow');
    const activeInput = document.getElementById('profileActive');

    const titleInput = document.getElementById('titleInput');
    const titleTagsContainer = document.getElementById('titleTags');
    const locationInput = document.getElementById('locationInput');
    const locationTagsContainer = document.getElementById('locationTags');

    const intervalInput = document.getElementById('profileInterval');
    const maxAgeInput = document.getElementById('profileMaxAge');
    const chatIdInput = document.getElementById('telegramChatId');
    const botTokenInput = document.getElementById('botToken');
    const toggleBotTokenBtn = document.getElementById('toggleBotToken');
    const btnSubmitProfile = document.getElementById('btnSubmitProfile');
    const btnResetForm = document.getElementById('btnResetForm');

    const toast = document.getElementById('toast');
    const gateError = document.getElementById('gateError');
    const gateErrorText = document.getElementById('gateErrorText');
    const gatePassWrapper = gateKeyInput ? gateKeyInput.closest('.pass-wrapper') : null;
    const btnGateSubmit = document.getElementById('btnGateSubmit');

    function showGateError(msg = 'You had one job!') {
        if (!gateError) return;
        if (gateErrorText) gateErrorText.textContent = msg;
        gateError.style.display = 'inline-block';
        if (gatePassWrapper) gatePassWrapper.classList.add('input-error');
        gateError.classList.remove('shake');
        void gateError.offsetWidth; 
        gateError.classList.add('shake');
        gateKeyInput.focus();
    }

    function hideGateError() {
        if (gateError) gateError.style.display = 'none';
        if (gatePassWrapper) gatePassWrapper.classList.remove('input-error');
    }

    if (gateKeyInput) {
        gateKeyInput.addEventListener('input', hideGateError);
    }

    toggleGateKey.addEventListener('click', () => {
        const isPass = gateKeyInput.type === 'password';
        gateKeyInput.type = isPass ? 'text' : 'password';
        toggleGateKey.innerHTML = isPass ? ICONS.eyeOff : ICONS.eye;
    });

    async function verifyAndFetchProfiles(secret) {
        const base = apiUrlInput.value.trim().replace(/\/+$/, '');
        if (!base) {
            return { valid: false, reason: 'no_api_url' };
        }

        try {
            const res = await fetch(`${base}/api/SearchProfiles`, {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'X-Api-Key': secret
                }
            });

            if (res.status === 200) {
                let data = null;
                const contentType = res.headers.get('content-type') || '';
                if (contentType.includes('application/json')) {
                    data = await res.json();
                }
                return { valid: true, profiles: data };
            }

            if (res.status === 401 || res.status === 403) {
                return { valid: false, reason: 'unauthorized' };
            }

            return { valid: false, reason: `server_error_${res.status}` };
        } catch (netErr) {
            return { valid: false, reason: 'network_error', error: netErr };
        }
    }

    gateForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const secret = gateKeyInput.value.trim();
        if (!secret) {
            showGateError('You had one job!');
            return;
        }
        hideGateError();

        const submitBtn = btnGateSubmit || gateForm.querySelector('button[type="submit"]');
        const originalText = submitBtn ? submitBtn.textContent : 'rm -rf unemployment';
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.textContent = 'Verifying...';
        }

        try {
            const check = await verifyAndFetchProfiles(secret);
            if (check.valid) {
                saveSecret(secret, gateRemember.checked);
                unlockConsole(check.profiles);
            } else if (check.reason === 'unauthorized') {
                showGateError("I've seen better attempts.");
                gateKeyInput.select();
            } else if (check.reason === 'network_error') {
                showGateError('Unable to connect to API');
            } else if (check.reason === 'no_api_url') {
                showGateError('API URL is missing');
            } else {
                showGateError('Verification failed');
            }
        } finally {
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.textContent = originalText;
            }
        }
    });

    btnLock.addEventListener('click', () => {
        clearSecret();
        lockConsole();
    });

    class AntigravityBackground {
        constructor(canvasId) {
            this.canvas = document.getElementById(canvasId);
            if (!this.canvas) return;
            this.ctx = this.canvas.getContext('2d');
            this.particles = [];
            this.animId = null;
            this.mouse = { x: -2000, y: -2000, isHovering: false };
            this.numParticles = 75;
            this.maxDistance = 140;

            this.init();
        }

        init() {
            this.resize();
            window.addEventListener('resize', () => this.resize());

            window.addEventListener('mousemove', (e) => {
                this.mouse.x = e.clientX;
                this.mouse.y = e.clientY;
                this.mouse.isHovering = true;
            });

            window.addEventListener('mouseleave', () => {
                this.mouse.isHovering = false;
            });

            window.addEventListener('click', (e) => {
                this.spawnPulse(e.clientX, e.clientY);
            });

            this.createParticles();
        }

        resize() {
            if (!this.canvas) return;
            this.width = window.innerWidth;
            this.height = window.innerHeight;
            const dpr = window.devicePixelRatio || 1;
            this.canvas.width = this.width * dpr;
            this.canvas.height = this.height * dpr;
            if (this.ctx.resetTransform) {
                this.ctx.resetTransform();
            } else {
                this.ctx.setTransform(1, 0, 0, 1, 0, 0);
            }
            this.ctx.scale(dpr, dpr);
        }

        createParticles() {
            this.particles = [];
            for (let i = 0; i < this.numParticles; i++) {
                this.particles.push(this.newParticle());
            }
        }

        newParticle(x, y, vx, vy) {
            return {
                x: x !== undefined ? x : Math.random() * this.width,
                y: y !== undefined ? y : Math.random() * this.height,
                vx: vx !== undefined ? vx : (Math.random() - 0.5) * 0.18,
                vy: vy !== undefined ? vy : (Math.random() - 0.5) * 0.18,
                radius: Math.random() * 1.5 + 1,
                alpha: Math.random() * 0.35 + 0.15
            };
        }

        spawnPulse(x, y) {
            for (let i = 0; i < 5; i++) {
                const angle = Math.random() * Math.PI * 2;
                const speed = Math.random() * 0.5 + 0.15;
                this.particles.push(this.newParticle(
                    x,
                    y,
                    Math.cos(angle) * speed,
                    Math.sin(angle) * speed
                ));
            }
            if (this.particles.length > this.numParticles + 25) {
                this.particles.splice(0, 5);
            }
        }

        start() {
            if (this.animId) return;
            const step = () => {
                this.update();
                this.draw();
                this.animId = requestAnimationFrame(step);
            };
            this.animId = requestAnimationFrame(step);
        }

        stop() {
            if (this.animId) {
                cancelAnimationFrame(this.animId);
                this.animId = null;
            }
        }

        update() {
            const mouseRadius = 150;

            for (let i = 0; i < this.particles.length; i++) {
                const p = this.particles[i];

                p.x += p.vx;
                p.y += p.vy;

                if (p.x < -10) p.x = this.width + 10;
                else if (p.x > this.width + 10) p.x = -10;
                if (p.y < -10) p.y = this.height + 10;
                else if (p.y > this.height + 10) p.y = -10;

                if (this.mouse.isHovering) {
                    const dx = p.x - this.mouse.x;
                    const dy = p.y - this.mouse.y;
                    const dist = Math.hypot(dx, dy);
                    if (dist < mouseRadius && dist > 0) {
                        const force = (1 - dist / mouseRadius) * 0.25;
                        p.x += (dx / dist) * force;
                        p.y += (dy / dist) * force;
                    }
                }
            }
        }

        draw() {
            this.ctx.clearRect(0, 0, this.width, this.height);

            for (let i = 0; i < this.particles.length; i++) {
                const p1 = this.particles[i];

                for (let j = i + 1; j < this.particles.length; j++) {
                    const p2 = this.particles[j];
                    const dx = p1.x - p2.x;
                    const dy = p1.y - p2.y;
                    const dist = Math.hypot(dx, dy);

                    if (dist < this.maxDistance) {
                        const alpha = (1 - dist / this.maxDistance) * 0.14;
                        this.ctx.strokeStyle = `rgba(255, 255, 255, ${alpha})`;
                        this.ctx.lineWidth = 1;
                        this.ctx.beginPath();
                        this.ctx.moveTo(p1.x, p1.y);
                        this.ctx.lineTo(p2.x, p2.y);
                        this.ctx.stroke();
                    }
                }

                if (this.mouse.isHovering) {
                    const dx = p1.x - this.mouse.x;
                    const dy = p1.y - this.mouse.y;
                    const dist = Math.hypot(dx, dy);
                    if (dist < 140) {
                        const alpha = (1 - dist / 140) * 0.35;
                        this.ctx.strokeStyle = `rgba(255, 255, 255, ${alpha})`;
                        this.ctx.lineWidth = 1;
                        this.ctx.beginPath();
                        this.ctx.moveTo(p1.x, p1.y);
                        this.ctx.lineTo(this.mouse.x, this.mouse.y);
                        this.ctx.stroke();
                    }
                }

                this.ctx.fillStyle = `rgba(255, 255, 255, ${p1.alpha})`;
                this.ctx.beginPath();
                this.ctx.arc(p1.x, p1.y, p1.radius, 0, Math.PI * 2);
                this.ctx.fill();
            }
        }
    }

    const bgEngine = new AntigravityBackground('antigravityCanvas');

    function unlockConsole(initialProfiles = null) {
        gateView.style.display = 'none';
        dashboardApp.style.display = 'flex';
        bgEngine.stop();
        loadProfiles(initialProfiles);
    }

    function lockConsole() {
        hideGateError();
        dashboardApp.style.display = 'none';
        gateView.style.display = 'flex';
        gateKeyInput.value = '';
        gateKeyInput.type = 'password';
        toggleGateKey.innerHTML = ICONS.eye;
        bgEngine.start();
        gateKeyInput.focus();
    }

    const urlParams = new URLSearchParams(window.location.search);
    const paramApi = urlParams.get('api');
    if (paramApi) {
        apiUrlInput.value = paramApi.trim().replace(/\/+$/, '');
        localStorage.setItem(URL_STORAGE_KEY, apiUrlInput.value);
    } else {
        const savedUrl = localStorage.getItem(URL_STORAGE_KEY);
        if (savedUrl) apiUrlInput.value = savedUrl;
    }

    apiUrlInput.addEventListener('change', () => {
        const val = apiUrlInput.value.trim().replace(/\/+$/, '');
        apiUrlInput.value = val;
        localStorage.setItem(URL_STORAGE_KEY, val);
        loadProfiles();
    });

    toggleBotTokenBtn.addEventListener('click', () => {
        const isPass = botTokenInput.type === 'password';
        botTokenInput.type = isPass ? 'text' : 'password';
        toggleBotTokenBtn.innerHTML = isPass ? ICONS.eyeOff : ICONS.eye;
    });

    navProfiles.addEventListener('click', () => {
        navProfiles.classList.add('active');
        navBlacklist.classList.remove('active');
        viewProfiles.style.display = 'flex';
        viewBlacklist.style.display = 'none';
    });

    navBlacklist.addEventListener('click', () => {
        navBlacklist.classList.add('active');
        navProfiles.classList.remove('active');
        viewProfiles.style.display = 'none';
        viewBlacklist.style.display = 'flex';
    });

    titleInput.addEventListener('keydown', (e) => {
        if (e.key === 'Enter') {
            e.preventDefault();
            const val = titleInput.value.trim();
            if (val && !state.targetJobTitles.includes(val)) {
                state.targetJobTitles.push(val);
                renderTitleTags();
                titleInput.value = '';
            }
        }
    });

    locationInput.addEventListener('keydown', (e) => {
        if (e.key === 'Enter') {
            e.preventDefault();
            const val = locationInput.value.trim();
            if (val && !state.targetLocations.includes(val)) {
                state.targetLocations.push(val);
                renderLocationTags();
                locationInput.value = '';
            }
        }
    });

    function renderTitleTags() {
        titleTagsContainer.innerHTML = state.targetJobTitles.map((t, idx) => `
            <span class="tag-chip">
                <span>${escape(t)}</span>
                <button type="button" class="tag-discard" onclick="KlarioApp.removeTitle(${idx})" title="Remove" aria-label="Remove">&times;</button>
            </span>
        `).join('');
    }

    function renderLocationTags() {
        locationTagsContainer.innerHTML = state.targetLocations.map((l, idx) => `
            <span class="tag-chip">
                <span>${escape(l)}</span>
                <button type="button" class="tag-discard" onclick="KlarioApp.removeLocation(${idx})" title="Remove" aria-label="Remove">&times;</button>
            </span>
        `).join('');
    }

    btnNewProfile.addEventListener('click', () => {
        state.selectedId = null;
        renderSidebar();
        resetForm();
        nameInput.focus();
    });

    btnResetForm.addEventListener('click', () => {
        if (state.selectedId) {
            selectProfile(state.selectedId);
        } else {
            resetForm();
        }
    });

    btnDeleteProfile.addEventListener('click', () => {
        if (state.selectedId) {
            deleteProfile(state.selectedId);
        }
    });

    async function apiRequest(method, path, body = null) {
        const base = apiUrlInput.value.trim().replace(/\/+$/, '');
        if (!base) {
            showToast('Enter an API URL');
            return null;
        }

        const url = `${base}${path.startsWith('/') ? path : '/' + path}`;
        const headers = { 'Accept': 'application/json' };

        const currentSecret = getSavedSecret();
        if (currentSecret) {
            headers['X-Api-Key'] = currentSecret;
        }

        const options = {
            method: method.toUpperCase(),
            headers
        };

        if (body && ['POST', 'PUT', 'PATCH'].includes(options.method)) {
            headers['Content-Type'] = 'application/json';
            options.body = JSON.stringify(body);
        }

        try {
            const res = await fetch(url, options);

            if (res.status === 401) {
                showToast('Unauthorized: Invalid secret');
                clearSecret();
                lockConsole();
                return null;
            }

            if (!res.ok) {
                let errText = `HTTP ${res.status}`;
                try {
                    const errData = await res.json();
                    if (errData.message) errText = errData.message;
                } catch {}
                showToast(errText);
                return null;
            }

            const contentType = res.headers.get('content-type') || '';
            if (contentType.includes('application/json')) {
                return await res.json();
            }
            return await res.text();
        } catch (err) {
            showToast('Connection failed');
            return null;
        }
    }

    async function loadProfiles(initialProfiles = null) {
        let result = initialProfiles;
        if (!result) {
            result = await apiRequest('GET', '/api/SearchProfiles');
            if (!result) return;
        }

        let list = [];
        if (Array.isArray(result)) list = result;
        else if (result.data && Array.isArray(result.data)) list = result.data;

        state.profiles = list;
        renderSidebar();

        if (state.selectedId && state.profiles.some(p => p.id === state.selectedId)) {
            selectProfile(state.selectedId);
        } else if (state.profiles.length > 0) {
            selectProfile(state.profiles[0].id);
        } else {
            resetForm();
        }
    }

    profileSearch.addEventListener('input', (e) => {
        state.filter = e.target.value.toLowerCase().trim();
        renderSidebar();
    });

    function renderSidebar() {
        const query = state.filter;
        const filtered = state.profiles.filter(p => {
            if (!query) return true;
            return (p.name || '').toLowerCase().includes(query);
        });

        profileCount.textContent = `${state.profiles.length} profile${state.profiles.length === 1 ? '' : 's'}`;

        if (filtered.length === 0) {
            sidebarProfiles.innerHTML = `<div class="sidebar-empty">${state.profiles.length === 0 ? 'No profiles yet' : 'No matches'}</div>`;
            return;
        }

        sidebarProfiles.innerHTML = filtered.map(p => {
            const isSelected = p.id === state.selectedId;
            return `
                <div class="sidebar-item ${isSelected ? 'active' : ''}" data-id="${p.id}" onclick="KlarioApp.select('${p.id}')">
                    ${escape(p.name)}
                </div>
            `;
        }).join('');
    }

    function selectProfile(id) {
        const p = state.profiles.find(item => item.id === id);
        if (!p) return;

        state.selectedId = id;
        renderSidebar();
        populateForm(p);
    }

    function populateForm(p) {
        editIdInput.value = p.id;
        formHeading.textContent = p.name || 'Edit Profile';
        btnSubmitProfile.textContent = 'Save Changes';
        btnDeleteProfile.style.display = 'inline-block';
        activeRow.style.display = 'flex';

        nameInput.value = p.name || '';
        descInput.value = p.description || '';
        activeInput.checked = p.isActive !== false;

        state.targetJobTitles = [...(p.targetJobTitles || [])];
        state.targetLocations = [...(p.targetLocations || [])];
        renderTitleTags();
        renderLocationTags();

        setSelectedOptions('workplaceGroup', p.workplace);
        setSelectedOptions('experienceGroup', p.experience);
        setSelectedOptions('jobTypeGroup', p.jobType);

        intervalInput.value = p.intervalMinutes || 15;
        maxAgeInput.value = p.maxPostingAgeMinutes || 1440;
        chatIdInput.value = p.telegramChatId || '';
        botTokenInput.value = p.botToken || '';
        botTokenInput.type = 'password';
        toggleBotTokenBtn.innerHTML = ICONS.eye;
    }

    function resetForm() {
        editIdInput.value = '';
        formHeading.textContent = 'New Profile';
        btnSubmitProfile.textContent = 'Save Profile';
        btnDeleteProfile.style.display = 'none';
        activeRow.style.display = 'none';

        form.reset();
        state.targetJobTitles = [];
        state.targetLocations = [];
        renderTitleTags();
        renderLocationTags();

        botTokenInput.type = 'password';
        toggleBotTokenBtn.innerHTML = ICONS.eye;

        intervalInput.value = 15;
        maxAgeInput.value = 1440;
        setSelectedOptions('workplaceGroup', 'Remote');
        setSelectedOptions('experienceGroup', 'MidLevel');
        setSelectedOptions('jobTypeGroup', 'FullTime');
    }

    function getSelectedOptions(containerId) {
        const checked = Array.from(document.querySelectorAll(`#${containerId} input:checked`))
            .map(cb => cb.value);
        return checked.length > 0 ? checked.join(', ') : 'None';
    }

    function setSelectedOptions(containerId, valueStr) {
        const values = (valueStr || '').split(',').map(s => s.trim().toLowerCase());
        document.querySelectorAll(`#${containerId} input`).forEach(cb => {
            cb.checked = values.includes(cb.value.toLowerCase());
        });
    }

    form.addEventListener('submit', async (e) => {
        e.preventDefault();

        if (state.targetJobTitles.length === 0) {
            showToast('Add at least one target job title (press Enter)');
            titleInput.focus();
            return;
        }

        if (state.targetLocations.length === 0) {
            showToast('Add at least one target location (press Enter)');
            locationInput.focus();
            return;
        }

        const id = editIdInput.value;
        const name = nameInput.value.trim();
        const description = descInput.value.trim() || null;
        const targetJobTitles = [...state.targetJobTitles];
        const targetLocations = [...state.targetLocations];
        const workplace = getSelectedOptions('workplaceGroup');
        const experience = getSelectedOptions('experienceGroup');
        const jobType = getSelectedOptions('jobTypeGroup');
        const intervalMinutes = parseInt(intervalInput.value, 10) || 15;
        const maxPostingAgeMinutes = parseInt(maxAgeInput.value, 10) || 1440;
        const telegramChatId = chatIdInput.value.trim();
        const botToken = botTokenInput.value.trim();

        const payload = {
            name,
            description,
            targetJobTitles,
            targetLocations,
            maxPostingAgeMinutes,
            intervalMinutes,
            workplace,
            experience,
            jobType,
            telegramChatId,
            botToken
        };

        if (id) {
            payload.isActive = activeInput.checked;
            const res = await apiRequest('PUT', `/api/SearchProfiles/${id}`, payload);
            if (res) {
                showToast('Profile updated');
                await loadProfiles();
            }
        } else {
            const res = await apiRequest('POST', '/api/SearchProfiles', payload);
            if (res) {
                showToast('Profile created');
                if (res.id) state.selectedId = res.id;
                await loadProfiles();
            }
        }
    });

    async function deleteProfile(id) {
        const p = state.profiles.find(item => item.id === id);
        const name = p ? p.name : 'this profile';
        if (!confirm(`Delete ${name}?`)) return;

        const res = await apiRequest('DELETE', `/api/SearchProfiles/${id}`);
        if (res) {
            showToast('Profile deleted');
            state.selectedId = null;
            await loadProfiles();
        }
    }

    let toastTimeout;
    function showToast(msg) {
        toast.textContent = msg;
        toast.classList.add('show');
        clearTimeout(toastTimeout);
        toastTimeout = setTimeout(() => toast.classList.remove('show'), 3000);
    }

    function escape(str) {
        if (!str) return '';
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;');
    }

    window.KlarioApp = {
        select: selectProfile,
        removeTitle: (index) => {
            state.targetJobTitles.splice(index, 1);
            renderTitleTags();
        },
        removeLocation: (index) => {
            state.targetLocations.splice(index, 1);
            renderLocationTags();
        }
    };

    if (getSavedSecret()) {
        unlockConsole();
    } else {
        lockConsole();
    }
})();
