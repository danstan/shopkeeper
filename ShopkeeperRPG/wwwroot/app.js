document.addEventListener('DOMContentLoaded', () => {
    const playerNameInput = document.getElementById('playerNameInput');
    const createCharacterButton = document.getElementById('createCharacterButton');
    const characterDisplay = document.getElementById('characterDisplay');

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
                    goldElement.textContent = `Gold: ${character.gold}`; // Gold is already displayed
                    characterDisplay.appendChild(goldElement);

                    // Display Inventory
                    const inventoryElement = document.createElement('p');
                    if (character.inventory && character.inventory.length > 0) {
                        // This part is for future use if inventory can have items upon creation
                        let inventoryText = 'Inventory: ';
                        character.inventory.forEach((item, index) => {
                            inventoryText += `${item.name}${index < character.inventory.length - 1 ? ', ' : ''}`;
                        });
                        inventoryElement.textContent = inventoryText;
                    } else {
                        inventoryElement.textContent = 'Inventory: (empty)';
                    }
                    characterDisplay.appendChild(inventoryElement);

                } else {
                    const errorData = await response.json(); // Attempt to get error message from API
                    console.error('Error creating character:', response.status, errorData);
                    characterDisplay.innerHTML = `<p style="color: red;">Error: ${errorData.title || response.statusText}</p>`;
                    if (errorData.errors) { // Display validation errors if any
                        for (const key in errorData.errors) {
                            const errorP = document.createElement('p');
                            errorP.style.color = 'red';
                            errorP.textContent = `${key}: ${errorData.errors[key].join(', ')}`;
                            characterDisplay.appendChild(errorP);
                        }
                    } else if (typeof errorData === 'string') { // Handle plain string error responses
                         characterDisplay.innerHTML = `<p style="color: red;">Error: ${errorData}</p>`;
                    }
                }
            } catch (error) {
                console.error('Failed to send request:', error);
                characterDisplay.innerHTML = '<p style="color: red;">Failed to send request. See console for details.</p>';
            }
        });
    } else {
        console.error('#createCharacterButton not found.');
    }
});
