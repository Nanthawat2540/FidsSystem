
        // Flight Display System JavaScript
        class FlightDisplay {
            constructor() {
                this.currentTimeElement = document.getElementById('current-time');
                this.flights = [
                    {
                        airline: 'NOK AIR',
                        flightNumber: '571',
                        destination: 'U-TAPAO INTERNATIONAL AIRPORT',
                        scheduleTime: '14:40',
                        boardingTime: '17:00',
                        gate: '3',
                        status: 'CHECK-IN OPEN'
                    },
                    {
                        airline: 'NOK AIR',
                        flightNumber: 'DD571',
                        destination: 'SURAT THANI INTERNATIONAL AIRPORT',
                        scheduleTime: '14:40',
                        boardingTime: null,
                        gate: null,
                        status: 'SCHEDULED'
                    }
                ];
                
                this.init();
            }
            
            init() {
                this.updateCurrentTime();
                this.startTimeUpdate();
                this.setupEventListeners();
                this.startStatusAnimations();
            }
            
            updateCurrentTime() {
                const now = new Date();
                const options = {
                    weekday: 'short',
                    day: 'numeric',
                    month: 'short',
                    year: 'numeric',
                    hour: '2-digit',
                    minute: '2-digit',
                    hour12: false
                };
                
                const formattedTime = now.toLocaleString('en-GB', options)
                    .replace(',', '')
                    .toUpperCase();
                
                if (this.currentTimeElement) {
                    this.currentTimeElement.textContent = formattedTime;
                }
            }
            
            startTimeUpdate() {
                setInterval(() => {
                    this.updateCurrentTime();
                }, 1000);
            }
            
            setupEventListeners() {
                // Add hover effects for flight cards
                const flightCards = document.querySelectorAll('.current-flight, .next-flight');
                flightCards.forEach(card => {
                    card.addEventListener('mouseenter', () => {
                        card.style.transform = 'translateX(10px)';
                        card.style.transition = 'transform 0.3s ease';
                    });
                    
                    card.addEventListener('mouseleave', () => {
                        card.style.transform = 'translateX(0)';
                    });
                });
            }
            
            startStatusAnimations() {
                // Simulate boarding time countdown
                this.startBoardingCountdown();
            }
            
            startBoardingCountdown() {
                const boardingTimeElement = document.querySelector('.time-value');
                if (boardingTimeElement) {
                    const boardingTime = boardingTimeElement.textContent;
                    const [hours, minutes] = boardingTime.split(':').map(Number);
                    const now = new Date();
                    const boarding = new Date();
                    boarding.setHours(hours, minutes, 0, 0);
                    
                    // If boarding time has passed, set it for tomorrow
                    if (boarding <= now) {
                        boarding.setDate(boarding.getDate() + 1);
                    }
                    
                    setInterval(() => {
                        const now = new Date();
                        const timeUntilBoarding = boarding - now;
                        
                        if (timeUntilBoarding > 0) {
                            const hoursLeft = Math.floor(timeUntilBoarding / (1000 * 60 * 60));
                            const minutesLeft = Math.floor((timeUntilBoarding % (1000 * 60 * 60)) / (1000 * 60));
                            
                            if (hoursLeft < 1 && minutesLeft < 30) {
                                boardingTimeElement.style.color = '#FF4500';
                                boardingTimeElement.style.animation = 'pulse 1s infinite';
                            }
                        }
                    }, 60000); // Update every minute
                }
            }
            
            // Method to add announcement functionality
            makeAnnouncement(message, type = 'info') {
                const announcement = document.createElement('div');
                announcement.className = `announcement ${type}`;
                announcement.textContent = message;
                
                const colors = {
                    info: '#00BFFF',
                    warning: '#FFD700',
                    alert: '#FF4500',
                    success: '#00FF00'
                };
                
                announcement.style.cssText = `
                    position: fixed;
                    top: 50%;
                    left: 50%;
                    transform: translate(-50%, -50%);
                    background: ${colors[type]};
                    color: #000;
                    padding: 3vh 4vw;
                    border-radius: 15px;
                    font-size: min(2vw, 28px);
                    font-weight: bold;
                    z-index: 1000;
                    animation: fadeIn 0.5s ease;
                    text-align: center;
                    min-width: 30vw;
                    box-shadow: 0 20px 50px rgba(0, 0, 0, 0.3);
                `;
                
                document.body.appendChild(announcement);
                
                // Remove announcement after 5 seconds
                setTimeout(() => {
                    announcement.style.animation = 'fadeOut 0.5s ease';
                    setTimeout(() => {
                        document.body.removeChild(announcement);
                    }, 500);
                }, 5000);
            }
        }

        // Full screen functionality
        function toggleFullscreen() {
            if (!document.fullscreenElement) {
                document.documentElement.requestFullscreen().catch(err => {
                    console.log(`Error attempting to enable full-screen mode: ${err.message}`);
                });
            } else {
                document.exitFullscreen();
            }
        }

        // Gate information function
        function showGateInfo(gateNumber) {
            window.flightDisplay.makeAnnouncement(`Proceeding to Gate ${gateNumber}`, 'info');
        }

        // Initialize the flight display when the page loads
        document.addEventListener('DOMContentLoaded', () => {
            const flightDisplay = new FlightDisplay();
            
            // Make it globally accessible
            window.flightDisplay = flightDisplay;
        });

        // Keyboard shortcuts
        document.addEventListener('keydown', (e) => {
            if (e.key === 'F11') {
                e.preventDefault();
                toggleFullscreen();
            }
            
            if (e.ctrlKey && e.key === 'a') {
                e.preventDefault();
                window.flightDisplay.makeAnnouncement('Flight NOK AIR 571 is now boarding!', 'info');
            }
            
            if (e.ctrlKey && e.key === 'w') {
                e.preventDefault();
                window.flightDisplay.makeAnnouncement('Weather delay - Please standby', 'warning');
            }
        });

        // Handle fullscreen changes
        document.addEventListener('fullscreenchange', () => {
            const btn = document.querySelector('.fullscreen-btn');
            if (document.fullscreenElement) {
                btn.textContent = '⤋ Exit Full Screen';
            } else {
                btn.textContent = '⛶ Full Screen';
            }
        });
