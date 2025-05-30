document.addEventListener('DOMContentLoaded', () => {
    const playerNameInput = document.getElementById('playerNameInput');
    const createCharacterButton = document.getElementById('createCharacterButton');
    const getStarterItemButton = document.getElementById('getStarterItemButton');
    const visitMarketButton = document.getElementById('visitMarketButton');
    const runShopStandardButton = document.getElementById('runShopStandardButton'); 
    const longRestButton = document.getElementById('longRestButton'); 
    const runShopBarterButton = document.getElementById('runShopBarterButton'); 
    const characterDisplay = document.getElementById('characterDisplay');
    const marketDisplay = document.getElementById('marketDisplay');
    const shopMessageDisplay = document.getElementById('shopMessageDisplay'); 
    const barterUIDisplay = document.getElementById('barterUIDisplay'); 

    // Helper function to display character details
    function displayCharacter(character) {
        characterDisplay.innerHTML = ''; 
        const nameElement = document.createElement('p');
        nameElement.textContent = `Name: ${character.name}`;
        characterDisplay.appendChild(nameElement);
        const strengthElement = document.createElement('p');
        strengthElement.textContent = `Strength: ${character.strength}`;
        characterDisplay.appendChild(strengthElement);
        const dexterityElement = document.createElement('p');
        dexterityElement.textContent = `Dexterity: ${character.dexterity}`;
        characterDisplay.appendChild(dexterityElement);
        const constitutionElement = document.createElement('p');
        constitutionElement.textContent = `Constitution: ${character.constitution}`;
        characterDisplay.appendChild(constitutionElement);
        const intelligenceElement = document.createElement('p');
        intelligenceElement.textContent = `Intelligence: ${character.intelligence}`;
        characterDisplay.appendChild(intelligenceElement);
        const wisdomElement = document.createElement('p');
        wisdomElement.textContent = `Wisdom: ${character.wisdom}`;
        characterDisplay.appendChild(wisdomElement);
        const charismaElement = document.createElement('p');
        charismaElement.textContent = `Charisma: ${character.charisma}`;
        characterDisplay.appendChild(charismaElement);
        const goldElement = document.createElement('p');
        goldElement.textContent = `Gold: ${character.gold}`;
        characterDisplay.appendChild(goldElement);
        const inventoryElement = document.createElement('p');
        let inventoryHTML = '<strong>Inventory:</strong> ';
        if (character.inventory && character.inventory.length > 0) {
            inventoryHTML += character.inventory.map(item => item.name).join(', ');
        } else {
            inventoryHTML += 'None';
        }
        inventoryElement.innerHTML = inventoryHTML;
        characterDisplay.appendChild(inventoryElement);
        const dayElement = document.createElement('p');
        dayElement.textContent = `Day: ${character.currentDay}`;
        characterDisplay.appendChild(dayElement);
        const hourElement = document.createElement('p');
        hourElement.textContent = `Hour: ${character.currentHour}`;
        characterDisplay.appendChild(hourElement);
        const exhaustionElement = document.createElement('p');
        exhaustionElement.textContent = `Exhaustion: ${character.exhaustionLevel}`;
        characterDisplay.appendChild(exhaustionElement);
    }

    function disableActionButtons() {
        if(getStarterItemButton) getStarterItemButton.disabled = true;
        if(visitMarketButton) visitMarketButton.disabled = true;
        if(runShopStandardButton) runShopStandardButton.disabled = true;
        if(longRestButton) longRestButton.disabled = true; 
        if(runShopBarterButton) runShopBarterButton.disabled = true; 
    }

    function enableActionButtons() {
        if(getStarterItemButton) getStarterItemButton.disabled = false;
        if(visitMarketButton) visitMarketButton.disabled = false;
        if(runShopStandardButton) runShopStandardButton.disabled = false;
        if(longRestButton) longRestButton.disabled = false; 
        if(runShopBarterButton) runShopBarterButton.disabled = false; 
    }
    
    disableActionButtons(); 

    // --- Barter Action Handler ---
    async function handleBarterAction(action, counterAmount = null) {
        const requestBody = { action: action };
        if (counterAmount !== null) {
            requestBody.counterAmount = parseInt(counterAmount, 10);
            if (isNaN(requestBody.counterAmount) || requestBody.counterAmount <= 0) {
                barterUIDisplay.innerHTML = '<p style="color: red;">Invalid counter amount. Please enter a positive number.</p>';
                // Optionally, re-render the previous barter state here if you stored it or re-fetch initiate data.
                // For now, just showing an error. The barter UI will be cleared by the next successful initiate.
                return;
            }
        }

        try {
            const response = await fetch('/api/player/barter/respond', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(requestBody)
            });

            const responseData = await response.json(); 

            if (response.ok) {
                barterUIDisplay.innerHTML = `<p>${responseData.message}</p>`; 
                if (responseData.updatedPlayer) {
                    displayCharacter(responseData.updatedPlayer); 
                }
                // If barterEnded is true (which it is in the current backend logic for all responses),
                // the barter UI is effectively cleared by displaying the message.
                // If the backend were to support multiple turns, we'd update the UI here
                // with new NPC offer or options based on responseData.npcResponseAction and responseData.newNpcOffer.
            } else {
                const errorMsg = responseData.message || responseData.title || 'An unknown error occurred during barter response.';
                barterUIDisplay.innerHTML = `<p style="color: red;">Barter response failed: ${errorMsg}</p>`;
                if (responseData.updatedPlayer) { // Still update player if data is sent with error
                    displayCharacter(responseData.updatedPlayer);
                }
            }
        } catch (error) {
            console.error('Error in handleBarterAction:', error);
            barterUIDisplay.innerHTML = '<p style="color: red;">Network error or other issue processing barter action.</p>';
        }
    }


    if (createCharacterButton) {
        createCharacterButton.addEventListener('click', async () => {
            const playerNameValue = playerNameInput.value;
            if (!playerNameValue.trim()) {
                alert('Please enter a character name.');
                return;
            }
            shopMessageDisplay.innerHTML = ''; 
            marketDisplay.innerHTML = ''; 
            barterUIDisplay.innerHTML = ''; 
            try {
                const response = await fetch('/api/player/create', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ name: playerNameValue }),
                });
                if (response.ok) {
                    const character = await response.json();
                    displayCharacter(character);
                    enableActionButtons();
                } else {
                    let errorMsg = `Error: ${response.statusText}`;
                    try {
                        const errorData = await response.json();
                        errorMsg = errorData.title || errorData.message || (typeof errorData === 'string' ? errorData : errorMsg);
                        if (errorData.errors) {
                           for (const key in errorData.errors) { errorMsg += `\n${key}: ${errorData.errors[key].join(', ')}`; }
                        }
                    } catch (e) { /* Ignore */ }
                    console.error('Error creating character:', response.status, errorMsg);
                    characterDisplay.innerHTML = `<p style="color: red;">${errorMsg.replace(/\n/g, '<br>')}</p>`;
                    disableActionButtons();
                }
            } catch (error) {
                console.error('Failed to send character creation request:', error);
                characterDisplay.innerHTML = '<p style="color: red;">Failed to send request. See console for details.</p>';
                disableActionButtons();
            }
        });
    }

    if (getStarterItemButton) {
        getStarterItemButton.addEventListener('click', async () => {
            shopMessageDisplay.innerHTML = ''; 
            barterUIDisplay.innerHTML = '';
            try {
                const response = await fetch('/api/player/getstarteritem', { method: 'POST' });
                if (response.ok) {
                    const updatedPlayer = await response.json();
                    displayCharacter(updatedPlayer);
                } else {
                    const errorText = await response.text();
                    alert(`Could not get starter item: ${errorText || response.statusText}`);
                }
            } catch (error) {
                alert('Failed to send request for starter item. See console for details.');
            }
        });
    }

    if (visitMarketButton) {
        visitMarketButton.addEventListener('click', async () => {
            shopMessageDisplay.innerHTML = ''; 
            barterUIDisplay.innerHTML = '';
            try {
                const response = await fetch('/api/player/marketitems');
                if (response.ok) {
                    const marketItems = await response.json();
                    marketDisplay.innerHTML = ''; 
                    const marketHeading = document.createElement('h3');
                    marketHeading.textContent = 'Market';
                    marketDisplay.appendChild(marketHeading);
                    if (marketItems && marketItems.length > 0) {
                        marketItems.forEach(item => {
                            const itemDiv = document.createElement('div');
                            itemDiv.style.border = '1px solid #ccc'; itemDiv.style.padding = '10px'; itemDiv.style.marginBottom = '10px';
                            const itemNameElement = document.createElement('h4'); itemNameElement.textContent = item.name; itemDiv.appendChild(itemNameElement);
                            const itemDesc = document.createElement('p'); itemDesc.textContent = item.description; itemDiv.appendChild(itemDesc);
                            const itemPrice = document.createElement('p'); itemPrice.textContent = `Price: ${item.price} Gold`; itemDiv.appendChild(itemPrice);
                            const buyButton = document.createElement('button'); buyButton.textContent = 'Buy'; buyButton.className = 'buyButton'; buyButton.dataset.itemName = item.name; itemDiv.appendChild(buyButton);
                            marketDisplay.appendChild(itemDiv);
                        });
                    } else {
                        marketDisplay.appendChild(document.createElement('p')).textContent = 'The market is empty today.';
                    }
                } else {
                    const errorText = await response.text();
                    marketDisplay.innerHTML = `<p style="color: red;">Error fetching market items: ${errorText || response.statusText}</p>`;
                }
            } catch (error) {
                marketDisplay.innerHTML = '<p style="color: red;">Failed to fetch market items. See console for details.</p>';
            }
        });
    }

    if (marketDisplay) {
        marketDisplay.addEventListener('click', async (event) => {
            if (event.target.classList.contains('buyButton')) {
                event.preventDefault(); 
                const itemName = event.target.dataset.itemName;
                if (!itemName) { alert('Could not determine which item to buy.'); return; }
                shopMessageDisplay.innerHTML = ''; 
                barterUIDisplay.innerHTML = '';
                try {
                    const response = await fetch('/api/player/buyitem', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ itemName: itemName }),
                    });
                    if (response.ok) {
                        const updatedPlayer = await response.json();
                        displayCharacter(updatedPlayer);
                    } else {
                        let errorMsg = `Error: ${response.statusText}`;
                         try {
                            const errorData = await response.json();
                            errorMsg = (typeof errorData === 'string' && errorData) ? errorData : (errorData.message || errorData.title || errorMsg);
                        } catch (e) { try { errorMsg = await response.text(); if (!errorMsg) errorMsg = `Error: ${response.statusText}`; } catch (textError) { errorMsg = `Error: ${response.statusText}`; } }
                        alert(`Could not buy item: ${errorMsg}`);
                    }
                } catch (error) {
                    alert('Failed to send purchase request. See console for details.');
                }
            }
        });
    }

    if (runShopStandardButton) {
        runShopStandardButton.addEventListener('click', async () => {
            shopMessageDisplay.innerHTML = ''; 
            marketDisplay.innerHTML = ''; 
            barterUIDisplay.innerHTML = '';
            try {
                const response = await fetch('/api/player/runshopstandard', { method: 'POST' });
                const responseData = await response.json(); 
                if (response.ok) {
                    shopMessageDisplay.textContent = responseData.message;
                    if (responseData.updatedPlayer) displayCharacter(responseData.updatedPlayer);
                } else {
                    const errorMessage = responseData.message || responseData.title || (typeof responseData === 'string' ? responseData : "Error running shop.");
                    shopMessageDisplay.innerHTML = `<p style="color: red;">${errorMessage}</p>`;
                    if (responseData.updatedPlayer) displayCharacter(responseData.updatedPlayer);
                }
            } catch (error) {
                shopMessageDisplay.innerHTML = '<p style="color: red;">Failed to run shop. See console for details.</p>';
            }
        });
    }

    if (longRestButton) {
        longRestButton.addEventListener('click', async () => {
            shopMessageDisplay.innerHTML = ''; 
            marketDisplay.innerHTML = '';    
            barterUIDisplay.innerHTML = '';
            try {
                const response = await fetch('/api/player/longrest', { method: 'POST' });
                if (response.ok) {
                    const updatedPlayer = await response.json();
                    displayCharacter(updatedPlayer);
                    shopMessageDisplay.textContent = "You feel rested and rejuvenated."; 
                } else {
                    const errorText = await response.text();
                    alert(`Could not take long rest: ${errorText || response.statusText}`);
                }
            } catch (error) {
                alert('Failed to send request for long rest. See console for details.');
            }
        });
    }

    if (runShopBarterButton) {
        runShopBarterButton.addEventListener('click', async () => {
            shopMessageDisplay.innerHTML = ''; 
            marketDisplay.innerHTML = '';    
            barterUIDisplay.innerHTML = '';   

            try {
                const response = await fetch('/api/player/barter/initiate', { method: 'POST' });
                const initiateData = await response.json(); 

                if (response.ok) {
                    console.log('Barter initiated:', initiateData);
                    if (initiateData.itemName == null) { 
                        barterUIDisplay.innerHTML = `<p>${initiateData.message}</p>`;
                        if (initiateData.updatedPlayer) { displayCharacter(initiateData.updatedPlayer); }
                        return; 
                    }
                    const heading = document.createElement('h4'); heading.textContent = initiateData.message; barterUIDisplay.appendChild(heading);
                    const itemDetails = document.createElement('p'); itemDetails.textContent = `Item: ${initiateData.itemName} (${initiateData.itemDescription})`; barterUIDisplay.appendChild(itemDetails);
                    const npcOffer = document.createElement('p'); npcOffer.textContent = `NPC's Offer: ${initiateData.npcInitialOffer} Gold`; barterUIDisplay.appendChild(npcOffer);
                    const acceptButton = document.createElement('button'); acceptButton.id = 'barterAcceptButton'; acceptButton.textContent = 'Accept Offer'; barterUIDisplay.appendChild(acceptButton);
                    const denyButton = document.createElement('button'); denyButton.id = 'barterDenyButton'; denyButton.textContent = 'Deny Offer'; barterUIDisplay.appendChild(denyButton);
                    const counterDiv = document.createElement('div'); counterDiv.style.marginTop = '10px';
                    const counterLabel = document.createElement('label'); counterLabel.htmlFor = 'barterCounterAmountInput'; counterLabel.textContent = 'Your Counter Offer (Gold): '; counterDiv.appendChild(counterLabel);
                    const counterInput = document.createElement('input'); counterInput.type = 'number'; counterInput.id = 'barterCounterAmountInput'; counterInput.min = '1'; counterInput.style.marginLeft = '5px'; counterDiv.appendChild(counterInput);
                    const submitCounterButton = document.createElement('button'); submitCounterButton.id = 'barterSubmitCounterButton'; submitCounterButton.textContent = 'Submit Counter'; submitCounterButton.style.marginLeft = '5px'; counterDiv.appendChild(submitCounterButton);
                    barterUIDisplay.appendChild(counterDiv);
                    if (initiateData.updatedPlayer) { displayCharacter(initiateData.updatedPlayer); }
                } else {
                    const errorMessage = initiateData.message || initiateData.title || (typeof initiateData === 'string' ? initiateData : "Error initiating barter.");
                    barterUIDisplay.innerHTML = `<p style="color: red;">${errorMessage}</p>`;
                }
            } catch (error) {
                console.error('Failed to send initiate barter request:', error);
                barterUIDisplay.innerHTML = '<p style="color: red;">Failed to initiate barter. See console for details.</p>';
            }
        });
    }

    // Event delegation for Barter UI buttons
    if (barterUIDisplay) {
        barterUIDisplay.addEventListener('click', async (event) => {
            const targetId = event.target.id;
            if (targetId === 'barterAcceptButton') {
                handleBarterAction('accept');
            } else if (targetId === 'barterDenyButton') {
                handleBarterAction('deny');
            } else if (targetId === 'barterSubmitCounterButton') {
                const counterInput = document.getElementById('barterCounterAmountInput');
                if (counterInput) {
                    handleBarterAction('counter', counterInput.value);
                } else {
                    console.error('#barterCounterAmountInput not found.');
                    barterUIDisplay.innerHTML = '<p style="color: red;">Error: Counter input field missing.</p>';
                }
            }
        });
    } else {
        console.error('#barterUIDisplay not found for event delegation.');
    }
});
