// script.js
class BaggageClaimDisplay {
    constructor() {
        this.flights = [
            { number: 'FD3239', airline: 'airasia', status: 'open' },
            { number: 'FD3235', airline: 'airasia', status: 'open' },
            { number: 'SL734', airline: 'lionair', status: 'open' },
            { number: 'FD3237', airline: 'airasia', status: 'open' }
        ];
        
        this.currentLanguage = 0;
        this.languages = ['en', 'th', 'zh'];
        this.displaySize = this.detectDisplaySize();
        
        this.translations = {
            'surat-thani': {
                'en': 'SURAT THANI<br>INTERNATIONAL AIRPORT',
                'th': 'ท่าอากาศยานนานาชาติ<br>สุราษฎร์ธานี',
                'zh': '素叻他尼<br>国际机场'
            }
        };
        
        this.init();
    }
    
    detectDisplaySize() {
        const width = window.innerWidth;
        const height = window.innerHeight;
        
        console.log(`Display Resolution: ${width}x${height}`);
        
        if (width >= 3500) {
            console.log('Display Size: 55-inch (4K)');
            return '55inch';
        } else if (width >= 1800 && width <= 2000) {
            console.log('Display Size: 43-inch (Full HD)');
            return '43inch';
        } else {
            console.log('Display Size: Standard/Mobile');
            return 'standard';
        }
    }
    
    init() {
        this.updateTime();
        this.setInterval();
        this.addEventListeners();
        this.optimizeForDisplay();
    }
    
    optimizeForDisplay() {
        const body = document.body;
        body.setAttribute('data-display-size', this.displaySize);
        
        // Add display-specific optimizations
        if (this.displaySize === '55inch') {
            // Optimize for 4K displays
            body.style.fontSize = '24px';
        } else if (this.displaySize === '43inch') {
            // Optimize for Full HD displays
            body.style.fontSize = '18px';
        } else {
            // Standard/mobile optimization
            body.style.fontSize = '16px';
        }
    }
    
    updateTime() {
        const now = new Date();
        const options = {
            weekday: 'short',
            day: '2-digit',
            month: 'short',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
            hour12: false
        };
        
        const formattedTime = now.toLocaleDateString('en-GB', options)
            .replace(/,/g, '')
            .toUpperCase();
        
        const footerRight = document.querySelector('.footer-right');
        if (footerRight) {
            footerRight.textContent = formattedTime;
        }
    }
    
    setInterval() {
        // Update time every minute
        setInterval(() => {
            this.updateTime();
        }, 60000);
        
        // Change language every 10 seconds
        setInterval(() => {
            this.switchLanguage();
        }, 10000);
        
        // Simulate status changes every 30 seconds
        setInterval(() => {
            this.simulateStatusChange();
        }, 30000);
    }
    
    switchLanguage() {
        const airportElements = document.querySelectorAll('.airport');
        
        // Fade out
        airportElements.forEach(element => {
            element.classList.add('fade-out');
        });
        
        // Change text after fade out
        setTimeout(() => {
            this.currentLanguage = (this.currentLanguage + 1) % this.languages.length;
            const currentLang = this.languages[this.currentLanguage];
            
            airportElements.forEach(element => {
                const airportKey = element.getAttribute('data-airport');
                if (this.translations[airportKey]) {
                    element.innerHTML = this.translations[airportKey][currentLang];
                }
                
                // Fade in
                element.classList.remove('fade-out');
                element.classList.add('fade-in');
            });
            
            // Remove fade-in class after animation
            setTimeout(() => {
                airportElements.forEach(element => {
                    element.classList.remove('fade-in');
                });
            }, 500);
        }, 250);
    }
    
    simulateStatusChange() {
        const randomIndex = Math.floor(Math.random() * this.flights.length);
        const flight = this.flights[randomIndex];
        
        // Toggle status
        flight.status = flight.status === 'open' ? 'closed' : 'open';
        
        // Update UI
        this.updateFlightStatus(randomIndex, flight.status);
    }
    
    updateFlightStatus(index, status) {
        const flightRows = document.querySelectorAll('.flight-row');
        const statusBadge = flightRows[index].querySelector('.status-badge');
        
        statusBadge.className = `status-badge ${status}`;
        statusBadge.textContent = status.toUpperCase();
        
        // Add flash effect
        statusBadge.style.animation = 'none';
        setTimeout(() => {
            statusBadge.style.animation = status === 'open' ? 'pulse 2s infinite' : 'none';
        }, 100);
    }
    
    addEventListeners() {
        // Add click handlers for flight rows
        const flightRows = document.querySelectorAll('.flight-row');
        flightRows.forEach((row, index) => {
            row.addEventListener('click', () => {
                this.handleFlightClick(index);
            });
        });
        
        // Add keyboard navigation
        document.addEventListener('keydown', (e) => {
            this.handleKeyPress(e);
        });
    }
    
    handleFlightClick(index) {
        const flight = this.flights[index];
        
        // Show flight details (could be expanded to show more info)
        console.log(`Flight ${flight.number} clicked`);
        
        // Add visual feedback
        const flightRow = document.querySelectorAll('.flight-row')[index];
        flightRow.style.backgroundColor = '#222';
        setTimeout(() => {
            flightRow.style.backgroundColor = '';
        }, 200);
    }
    
    handleKeyPress(e) {
        switch(e.key) {
            case 'r':
            case 'R':
                // Refresh display
                this.refreshDisplay();
                break;
            case 't':
            case 'T':
                // Toggle all statuses
                this.toggleAllStatuses();
                break;
            case 'l':
            case 'L':
                // Manual language switch
                this.manualSwitchLanguage();
                break;
        }
    }
    
    refreshDisplay() {
        // Simulate data refresh
        console.log('Refreshing display...');
        
        // Add visual feedback
        const container = document.querySelector('.display-container');
        container.style.opacity = '0.5';
        setTimeout(() => {
            container.style.opacity = '1';
            this.updateTime();
        }, 500);
    }
    
    toggleAllStatuses() {
        this.flights.forEach((flight, index) => {
            flight.status = flight.status === 'open' ? 'closed' : 'open';
            this.updateFlightStatus(index, flight.status);
        });
    }
    
    // Method to add new flight
    addFlight(flightNumber, airline, status = 'open') {
        this.flights.push({ number: flightNumber, airline: airline, status: status });
        this.renderFlights();
    }
    
    // Method to remove flight
    removeFlight(index) {
        this.flights.splice(index, 1);
        this.renderFlights();
    }
    
    renderFlights() {
        const container = document.querySelector('.flights-container');
        container.innerHTML = '';
        
        this.flights.forEach((flight, index) => {
            const flightRow = document.createElement('div');
            flightRow.className = 'flight-row';
            
            const currentLang = this.languages[this.currentLanguage];
            const airportText = this.translations['surat-thani'][currentLang];
            
            flightRow.innerHTML = `
                <div class="airline-logo ${flight.airline}"></div>
                <div class="flight-info">
                    <div class="flight-number">${flight.number}</div>
                    <div class="airport" data-airport="surat-thani">${airportText}</div>
                </div>
                <div class="status-badge ${flight.status}">${flight.status.toUpperCase()}</div>
            `;
            
            // Add click handler
            flightRow.addEventListener('click', () => {
                this.handleFlightClick(index);
            });
            
            container.appendChild(flightRow);
        });
    }
    
    // Method to manually switch language (for testing)
    manualSwitchLanguage() {
        this.switchLanguage();
    }
    
    // Method to get current language
    getCurrentLanguage() {
        return this.languages[this.currentLanguage];
    }
    
    // Method to add new airport translation
    addAirportTranslation(airportKey, translations) {
        this.translations[airportKey] = translations;
    }
}

// Initialize the display when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    const display = new BaggageClaimDisplay();
    
    // Make display globally accessible for debugging
    window.baggageDisplay = display;
    
    console.log('Baggage Claim Display initialized');
    console.log(`Display Size: ${display.displaySize}`);
    console.log('Press R to refresh, T to toggle all statuses, L to switch language manually');
    console.log('Airport names will automatically switch between English, Thai, and Chinese every 10 seconds');
});