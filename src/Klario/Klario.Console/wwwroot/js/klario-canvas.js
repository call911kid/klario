(function () {
    const activeTopos = new Map();

    class SwissTopographyEngine {
        constructor(canvasId, options = {}) {
            this.canvas = document.getElementById(canvasId);
            if (!this.canvas) return;
            this.ctx = this.canvas.getContext('2d');
            this.animId = null;

            this.r = 15;
            this.g = 118;
            this.b = 110;

            this.time = 0;
            this.speed = options.speed || 0.0038;
            this.numLines = options.lines || 8;
            this.lineWidth = options.lineWidth || 1.8;

            this.targetMouse = { x: -2000, y: -2000 };
            this.currentMouse = { x: -2000, y: -2000 };
            this.isHovering = false;
            this.hoverIntensity = 0;

            this.boundResize = () => this.resize();
            this.boundMouseMove = (e) => {
                this.targetMouse.x = e.clientX;
                this.targetMouse.y = e.clientY;
                if (!this.isHovering) {
                    this.isHovering = true;
                    if (this.currentMouse.x < -500) {
                        this.currentMouse.x = e.clientX;
                        this.currentMouse.y = e.clientY;
                    }
                }
            };
            this.boundMouseLeave = () => {
                this.isHovering = false;
            };

            this.init();
        }

        init() {
            this.resize();
            window.addEventListener('resize', this.boundResize, { passive: true });
            window.addEventListener('mousemove', this.boundMouseMove, { passive: true });
            window.addEventListener('mouseleave', this.boundMouseLeave, { passive: true });

            this.start();
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

        start() {
            if (this.animId) return;
            const step = () => {
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
            window.removeEventListener('resize', this.boundResize);
            window.removeEventListener('mousemove', this.boundMouseMove);
            window.removeEventListener('mouseleave', this.boundMouseLeave);
        }

        draw() {
            this.ctx.clearRect(0, 0, this.width, this.height);
            this.time += this.speed;

            const targetIntensity = this.isHovering ? 1 : 0;
            this.hoverIntensity += (targetIntensity - this.hoverIntensity) * 0.04;

            if (this.hoverIntensity > 0.005) {
                const dx = this.targetMouse.x - this.currentMouse.x;
                const dy = this.targetMouse.y - this.currentMouse.y;
                const dist = Math.hypot(dx, dy);

                if (dist > 0.5) {
                    const maxSpeed = 3.2;
                    const step = Math.min(dist * 0.035, maxSpeed);
                    this.currentMouse.x += (dx / dist) * step;
                    this.currentMouse.y += (dy / dist) * step;
                }
            }

            const stepY = this.height / (this.numLines + 1);
            this.ctx.lineWidth = this.lineWidth;

            for (let i = 1; i <= this.numLines; i++) {
                const baseY = i * stepY;
                const alpha = 0.20 + (i % 3) * 0.08;
                this.ctx.strokeStyle = `rgba(${this.r}, ${this.g}, ${this.b}, ${alpha})`;

                this.ctx.beginPath();
                this.ctx.moveTo(0, baseY);

                const stepX = 24;
                for (let x = 0; x <= this.width + stepX; x += stepX) {
                    const wave1 = Math.sin(x * 0.0035 + this.time + i * 0.85) * 28;
                    const wave2 = Math.cos(x * 0.007 - this.time * 0.5 + i * 0.45) * 14;

                    let mouseDeflection = 0;
                    if (this.hoverIntensity > 0.01) {
                        const mDist = Math.hypot(x - this.currentMouse.x, baseY - this.currentMouse.y);
                        if (mDist < 220) {
                            const bell = (Math.cos((mDist / 220) * Math.PI) + 1) * 0.5;
                            const breathe = 1 + Math.sin(this.time * 1.5 + mDist * 0.01) * 0.12;
                            mouseDeflection = bell * -22 * this.hoverIntensity * breathe;
                        }
                    }

                    const y = baseY + wave1 + wave2 + mouseDeflection;
                    this.ctx.lineTo(x, y);
                }

                this.ctx.stroke();
            }
        }
    }

    window.initKlarioTopography = function (canvasId, options = {}) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return false;

        if (activeTopos.has(canvasId)) {
            const existing = activeTopos.get(canvasId);
            if (existing && existing.canvas === canvas && existing.animId) {
                return true;
            }
            existing.stop();
            activeTopos.delete(canvasId);
        }

        const engine = new SwissTopographyEngine(canvasId, options);
        activeTopos.set(canvasId, engine);
        return true;
    };

    window.destroyKlarioTopography = function (canvasId) {
        if (activeTopos.has(canvasId)) {
            activeTopos.get(canvasId).stop();
            activeTopos.delete(canvasId);
        }
    };
})();
