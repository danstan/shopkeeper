document.addEventListener('DOMContentLoaded', () => {
    const playerNameInput = document.getElementById('playerNameInput');
    const createCharacterButton = document.getElementById('createCharacterButton');
    const getStarterItemButton = document.getElementById('getStarterItemButton');
    const characterDisplay = document.getElementById('characterDisplay');

    // Helper function to display character details
    function displayCharacter(character) {
        characterDisplay.innerHTML = ''; // Clear previous content

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

        // Updated Inventory Display
        const inventoryElement = document.createElement('p');
        let inventoryHTML = '<strong>Inventory:</strong> ';
        if (character.inventory && character.inventory.length > 0) {
            inventoryHTML += character.inventory.map(item => item.name).join(', ');
        } else {
            inventoryHTML += 'None';
        }
        inventoryElement.innerHTML = inventoryHTML; // Use innerHTML for the <strong> tag
        characterDisplay.appendChild(inventoryElement);
    }

    if (createCharacterButton) {
        createCharacterButton.addEventListener('click', async () => {
            const playerNameValue = playerNameInput.value;

            if (!playerNameValue.trim()) {
                alert('Please enter a character name.');
                return;
            }

            try {
                const response = await fetch('/api/player/create', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({ name: playerNameValue }),
                });

                if (response.ok) {
                    const character = await response.json();
                    displayCharacter(character);
                    if(getStarterItemButton) getStarterItemButton.disabled = false;
                } else {
                    const errorData = await response.json();
                    console.error('Error creating character:', response.status, errorData);
                    characterDisplay.innerHTML = `<p style="color: red;">Error: ${errorData.title || errorData.message || response.statusText}</p>`;
                     if (errorData.errors) {
                        for (const key in errorData.errors) {
                            const errorP = document.createElement('p');
                            errorP.style.color = 'red';
                            errorP.textContent = `${key}: ${errorData.errors[key].join(', ')}`;
                            characterDisplay.appendChild(errorP);
                        }
                    } else if (typeof errorData === 'string') {
                         characterDisplay.innerHTML = `<p style="color: red;">Error: ${errorData}</p>`;
                    }
                    if(getStarterItemButton) getStarterItemButton.disabled = true;
                }
            } catch (error) {
                console.error('Failed to send character creation request:', error);
                characterDisplay.innerHTML = '<p style="color: red;">Failed to send request. See console for details.</p>';
                if(getStarterItemButton) getStarterItemButton.disabled = true;
            }
        });
    } else {
        console.error('#createCharacterButton not found.');
    }

    if (getStarterItemButton) {
        getStarterItemButton.disabled = true; 

        getStarterItemButton.addEventListener('click', async () => {
            try {
                const response = await fetch('/api/player/getstarteritem', {
                    method: 'POST',
                });

                if (response.ok) {
                    const updatedPlayer = await response.json();
                    displayCharacter(updatedPlayer);
                } else {
                    const errorText = await response.text();
                    console.error('Error getting starter item:', response.status, errorText);
                    const errorP = document.createElement('p');
                    errorP.style.color = 'red';
                    errorP.textContent = `Error getting starter item: ${errorText || response.statusText}`;
                    characterDisplay.appendChild(errorP); 
                }
            } catch (error) {
                console.error('Failed to send get starter item request:', error);
                const errorP = document.createElement('p');
                errorP.style.color = 'red';
                errorP.textContent = 'Failed to send request for starter item. See console for details.';
                characterDisplay.appendChild(errorP);
            }
        });
    } else {
        console.error('#getStarterItemButton not found.');
    }
});
