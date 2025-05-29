document.addEventListener('DOMContentLoaded', () => {
    const playerNameInput = document.getElementById('playerNameInput');
    const createCharacterButton = document.getElementById('createCharacterButton');
    const getStarterItemButton = document.getElementById('getStarterItemButton');
    const visitMarketButton = document.getElementById('visitMarketButton');
    const runShopStandardButton = document.getElementById('runShopStandardButton'); // Get reference
    const characterDisplay = document.getElementById('characterDisplay');
    const marketDisplay = document.getElementById('marketDisplay');
    const shopMessageDisplay = document.getElementById('shopMessageDisplay'); // Get reference

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
    }

    // Helper function to disable all action buttons
    function disableActionButtons() {
        if(getStarterItemButton) getStarterItemButton.disabled = true;
        if(visitMarketButton) visitMarketButton.disabled = true;
        if(runShopStandardButton) runShopStandardButton.disabled = true;
    }

    // Helper function to enable action buttons (typically after character creation)
    function enableActionButtons() {
        if(getStarterItemButton) getStarterItemButton.disabled = false;
        if(visitMarketButton) visitMarketButton.disabled = false;
        if(runShopStandardButton) runShopStandardButton.disabled = false;
    }
    
    disableActionButtons(); // Disable buttons on page load

    if (createCharacterButton) {
        createCharacterButton.addEventListener('click', async () => {
            const playerNameValue = playerNameInput.value;
            if (!playerNameValue.trim()) {
                alert('Please enter a character name.');
                return;
            }
            shopMessageDisplay.innerHTML = ''; // Clear shop messages
            marketDisplay.innerHTML = ''; // Clear market display
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
                           for (const key in errorData.errors) {
                                errorMsg += `\n${key}: ${errorData.errors[key].join(', ')}`;
                           }
                        }
                    } catch (e) { /* Ignore if error response is not JSON */ }
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
    } else {
        console.error('#createCharacterButton not found.');
    }

    if (getStarterItemButton) {
        getStarterItemButton.addEventListener('click', async () => {
            shopMessageDisplay.innerHTML = ''; // Clear shop messages
            try {
                const response = await fetch('/api/player/getstarteritem', { method: 'POST' });
                if (response.ok) {
                    const updatedPlayer = await response.json();
                    displayCharacter(updatedPlayer);
                } else {
                    const errorText = await response.text();
                    console.error('Error getting starter item:', response.status, errorText);
                    alert(`Could not get starter item: ${errorText || response.statusText}`);
                }
            } catch (error) {
                console.error('Failed to send get starter item request:', error);
                alert('Failed to send request for starter item. See console for details.');
            }
        });
    } else {
        console.error('#getStarterItemButton not found.');
    }

    if (visitMarketButton) {
        visitMarketButton.addEventListener('click', async () => {
            shopMessageDisplay.innerHTML = ''; // Clear shop messages
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
                            itemDiv.style.border = '1px solid #ccc';
                            itemDiv.style.padding = '10px';
                            itemDiv.style.marginBottom = '10px';

                            const itemNameElement = document.createElement('h4');
                            itemNameElement.textContent = item.name;
                            itemDiv.appendChild(itemNameElement);

                            const itemDesc = document.createElement('p');
                            itemDesc.textContent = item.description;
                            itemDiv.appendChild(itemDesc);

                            const itemPrice = document.createElement('p');
                            itemPrice.textContent = `Price: ${item.price} Gold`;
                            itemDiv.appendChild(itemPrice);

                            const buyButton = document.createElement('button');
                            buyButton.textContent = 'Buy';
                            buyButton.className = 'buyButton';
                            buyButton.dataset.itemName = item.name;
                            itemDiv.appendChild(buyButton);

                            marketDisplay.appendChild(itemDiv);
                        });
                    } else {
                        const noItemsMessage = document.createElement('p');
                        noItemsMessage.textContent = 'The market is empty today.';
                        marketDisplay.appendChild(noItemsMessage);
                    }
                } else {
                    const errorText = await response.text();
                    console.error('Error fetching market items:', response.status, errorText);
                    marketDisplay.innerHTML = `<p style="color: red;">Error fetching market items: ${errorText || response.statusText}</p>`;
                }
            } catch (error) {
                console.error('Failed to send market items request:', error);
                marketDisplay.innerHTML = '<p style="color: red;">Failed to fetch market items. See console for details.</p>';
            }
        });
    } else {
        console.error('#visitMarketButton not found.');
    }

    if (marketDisplay) {
        marketDisplay.addEventListener('click', async (event) => {
            if (event.target.classList.contains('buyButton')) {
                event.preventDefault(); 
                const itemName = event.target.dataset.itemName;
                if (!itemName) {
                    console.error('Buy button clicked without item name.');
                    alert('Could not determine which item to buy.');
                    return;
                }
                shopMessageDisplay.innerHTML = ''; // Clear shop messages
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
                            errorMsg = (typeof errorData === 'string' && errorData) ? errorData : 
                                       (errorData.message || errorData.title || errorMsg);
                        } catch (e) {
                            try { errorMsg = await response.text(); if (!errorMsg) errorMsg = `Error: ${response.statusText}`; } 
                            catch (textError) { errorMsg = `Error: ${response.statusText}`; }
                        }
                        console.error('Error purchasing item:', response.status, errorMsg);
                        alert(`Could not buy item: ${errorMsg}`);
                    }
                } catch (error) {
                    console.error('Network error or issue purchasing item:', error);
                    alert('Failed to send purchase request. See console for details.');
                }
            }
        });
    } else {
        console.error('#marketDisplay not found for event delegation.');
    }

    // Event listener for the "Run Shop (Standard Rate)" button
    if (runShopStandardButton) {
        runShopStandardButton.addEventListener('click', async () => {
            shopMessageDisplay.innerHTML = ''; // Clear previous shop messages
            marketDisplay.innerHTML = ''; // Clear market display

            try {
                const response = await fetch('/api/player/runshopstandard', {
                    method: 'POST',
                    // No body needed for this request
                });

                const responseData = await response.json(); // Try to parse JSON regardless of response.ok

                if (response.ok) {
                    shopMessageDisplay.textContent = responseData.message;
                    if (responseData.updatedPlayer) {
                        displayCharacter(responseData.updatedPlayer);
                    }
                } else {
                    // Handle errors (e.g., player not found, etc.)
                    const errorMessage = responseData.message || responseData.title || (typeof responseData === 'string' ? responseData : "Error running shop.");
                    console.error('Error running shop:', response.status, errorMessage);
                    shopMessageDisplay.innerHTML = `<p style="color: red;">${errorMessage}</p>`;
                     // If player data was returned with the error (e.g. for "inventory empty" message), still update display
                    if (responseData.updatedPlayer) {
                        displayCharacter(responseData.updatedPlayer);
                    }
                }
            } catch (error) {
                console.error('Failed to send run shop request:', error);
                shopMessageDisplay.innerHTML = '<p style="color: red;">Failed to run shop. See console for details.</p>';
            }
        });
    } else {
        console.error('#runShopStandardButton not found.');
    }
});
