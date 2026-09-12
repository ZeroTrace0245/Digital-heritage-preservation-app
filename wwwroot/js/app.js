const API_BASE = 'http://localhost:5000/api';

// Initialize
document.addEventListener('DOMContentLoaded', () => {
    loadSection('dashboard');
    loadStats();
    setInterval(loadStats, 30000); // Refresh stats every 30 seconds
});

// Section Navigation
function loadSection(sectionId) {
    document.querySelectorAll('.section').forEach(s => s.classList.remove('active'));
    const section = document.getElementById(sectionId);
    if (section) {
        section.classList.add('active');
        window.scrollTo(0, 0);

        // Load data for the section
        if (sectionId === 'stories') loadStories();
        else if (sectionId === 'artifacts') loadArtifacts();
        else if (sectionId === 'languages') loadLanguages();
        else if (sectionId === 'events') loadEvents();
        else if (sectionId === 'community') { loadCommunity(); loadGroups(); loadProfile(); }
    }
}

function scrollToForm(formId) {
    setTimeout(() => {
        const form = document.getElementById(formId);
        if (form) form.scrollIntoView({ behavior: 'smooth' });
    }, 100);
}

// Community hub — the demo uses contributor 1 until authentication is added.
async function loadProfile() {
    try {
        const user = await fetch(`${API_BASE}/users/1`).then(r => r.json());
        ['Name', 'Email', 'Community', 'Bio'].forEach(field => {
            document.getElementById(`profile${field}`).value = user[field.charAt(0).toLowerCase() + field.slice(1)] || '';
        });
    } catch (error) { console.error('Error loading profile:', error); }
}

async function saveProfile() {
    const payload = {
        id: 1, name: profileName.value.trim(), email: profileEmail.value.trim(),
        community: profileCommunity.value.trim(), bio: profileBio.value.trim()
    };
    if (!payload.name || !payload.email) return alert('Name and email are required.');
    const response = await fetch(`${API_BASE}/users/1`, { method: 'PUT', headers: {'Content-Type': 'application/json'}, body: JSON.stringify(payload) });
    if (response.ok) alert('Your contributor profile was saved.'); else alert('Could not save your profile.');
}

async function addGroup() {
    const payload = { name: groupName.value.trim(), region: groupRegion.value.trim(), description: groupDescription.value.trim(), visibility: groupVisibility.value, createdById: 1 };
    if (!payload.name) return alert('Give the group a name.');
    const response = await fetch(`${API_BASE}/community/groups`, { method: 'POST', headers: {'Content-Type': 'application/json'}, body: JSON.stringify(payload) });
    if (response.ok) { groupForm.reset(); await loadGroups(); } else alert('Could not create the group.');
}

async function loadGroups() {
    try {
        const groups = await fetch(`${API_BASE}/community/groups`).then(r => r.json());
        groupsList.innerHTML = groups.length ? groups.map(group => `<div class="card"><h3>🏘️ ${escapeHtml(group.name)}</h3><span class="badge">${escapeHtml(group.visibility)}</span>${group.region ? `<p><strong>Area:</strong> ${escapeHtml(group.region)}</p>` : ''}<p>${escapeHtml(group.description || 'A heritage community space.')}</p></div>`).join('') : '<div class="empty-state"><p>No groups yet. Start the first one.</p></div>';
    } catch (error) { console.error('Error loading groups:', error); }
}

async function addCommunityItem() {
    const payload = {
        type: communityType.value, title: communityTitle.value.trim(), description: communityDescription.value.trim(), tags: communityTags.value.trim(),
        visibility: communityVisibility.value, consentConfirmed: communityConsent.checked, isSensitive: communitySensitive.checked, contributorId: 1
    };
    if (!payload.title || !payload.description) return alert('Add a title and description.');
    if (!payload.consentConfirmed) return alert('Please confirm you have consent before sharing.');
    const response = await fetch(`${API_BASE}/community/items`, { method: 'POST', headers: {'Content-Type': 'application/json'}, body: JSON.stringify(payload) });
    if (response.ok) { communityForm.reset(); await loadCommunity(); } else alert('Could not share this contribution.');
}

async function loadCommunity() {
    try {
        const type = document.getElementById('communityFilter').value;
        const items = await fetch(`${API_BASE}/community/items${type ? `?type=${encodeURIComponent(type)}` : ''}`).then(r => r.json());
        communityList.innerHTML = items.length ? items.map(item => {
            const verified = item.verifiedAt ? '<span class="badge verified">✓ Community verified</span>' : '';
            const sensitive = item.isSensitive ? '<span class="badge sensitive">⚠ Sensitive knowledge</span>' : '';
            const action = item.type === 'Identification' && !item.verifiedAt ? `<button class="btn btn-secondary btn-small" onclick="verifyCommunityItem(${item.id})">Mark verified</button>` : '';
            const close = ['Challenge', 'HelpRequest'].includes(item.type) && item.status !== 'Completed' ? `<button class="btn btn-secondary btn-small" onclick="closeCommunityItem(${item.id})">Mark complete</button>` : '';
            return `<div class="card"><span class="badge">${escapeHtml(item.type)}</span>${verified}${sensitive}<h3>${escapeHtml(item.title)}</h3><p>${escapeHtml(item.description)}</p>${item.tags ? `<p><strong>Tags:</strong> ${escapeHtml(item.tags)}</p>` : ''}<div class="meta"><p>${escapeHtml(item.visibility)} · ${escapeHtml(item.status)} · ${new Date(item.createdAt).toLocaleDateString()}</p></div><div class="actions">${action}${close}</div></div>`;
        }).join('') : '<div class="empty-state"><p>No community activity yet. Share a memory, question, or challenge.</p></div>';
    } catch (error) { console.error('Error loading community:', error); }
}

async function verifyCommunityItem(id) {
    const response = await fetch(`${API_BASE}/community/items/${id}/verify`, { method: 'POST' });
    if (response.ok) await loadCommunity();
}

async function closeCommunityItem(id) {
    const response = await fetch(`${API_BASE}/community/items/${id}/close`, { method: 'POST' });
    if (response.ok) await loadCommunity();
}

// Dashboard Stats
async function loadStats() {
    try {
        const [stories, artifacts, languages, events] = await Promise.all([
            fetch(`${API_BASE}/stories`).then(r => r.json()),
            fetch(`${API_BASE}/artifacts`).then(r => r.json()),
            fetch(`${API_BASE}/languages`).then(r => r.json()),
            fetch(`${API_BASE}/events`).then(r => r.json())
        ]);

        document.getElementById('storyCount').textContent = stories.length;
        document.getElementById('artifactCount').textContent = artifacts.length;
        document.getElementById('languageCount').textContent = languages.length;
        document.getElementById('eventCount').textContent = events.length;
    } catch (error) {
        console.error('Error loading stats:', error);
    }
}

// Stories
async function loadStories() {
    try {
        const response = await fetch(`${API_BASE}/stories`);
        const stories = await response.json();
        const list = document.getElementById('storiesList');

        if (stories.length === 0) {
            list.innerHTML = '<div class="empty-state"><p>No stories yet. Record the first one!</p></div>';
            return;
        }

        list.innerHTML = stories.map(story => `
            <div class="card">
                <h3>${escapeHtml(story.title)}</h3>
                <p>${escapeHtml(story.description)}</p>
                ${story.culture ? `<span class="badge">🌍 ${escapeHtml(story.culture)}</span>` : ''}
                ${story.language ? `<span class="badge">🗣️ ${escapeHtml(story.language)}</span>` : ''}
                ${story.location ? `<p><strong>Location:</strong> ${escapeHtml(story.location)}</p>` : ''}
                ${story.transcriptionText ? `<p><strong>Transcription:</strong> ${escapeHtml(story.transcriptionText.substring(0, 100))}...</p>` : ''}
                <div class="meta">
                    <p>Views: ${story.viewCount}</p>
                    <p>Recorded: ${new Date(story.createdAt).toLocaleDateString()}</p>
                </div>
                <div class="actions">
                    <button class="btn btn-secondary btn-small" onclick="editStory(${story.id})">Edit</button>
                    <button class="btn btn-danger btn-small" onclick="deleteStory(${story.id})">Delete</button>
                </div>
            </div>
        `).join('');
    } catch (error) {
        console.error('Error loading stories:', error);
    }
}

async function addStory() {
    const title = document.getElementById('storyTitle').value;
    const description = document.getElementById('storyDescription').value;
    const culture = document.getElementById('storyCulture').value;
    const language = document.getElementById('storyLanguage').value;
    const location = document.getElementById('storyLocation').value;
    const transcriptionText = document.getElementById('storyTranscription').value;

    if (!title || !description) {
        alert('Please fill in required fields');
        return;
    }

    try {
        const response = await fetch(`${API_BASE}/stories`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                title, description, culture, language, location, transcriptionText,
                mediaType: 'audio',
                contributorId: 1
            })
        });

        if (response.ok) {
            document.getElementById('storyForm').reset();
            await loadStories();
            await loadStats();
            alert('Story recorded successfully!');
        }
    } catch (error) {
        console.error('Error adding story:', error);
        alert('Error recording story');
    }
}

async function deleteStory(id) {
    if (!confirm('Are you sure you want to delete this story?')) return;

    try {
        const response = await fetch(`${API_BASE}/stories/${id}`, { method: 'DELETE' });
        if (response.ok) {
            await loadStories();
            await loadStats();
            alert('Story deleted successfully!');
        }
    } catch (error) {
        console.error('Error deleting story:', error);
    }
}

// Artifacts
async function loadArtifacts() {
    try {
        const response = await fetch(`${API_BASE}/artifacts`);
        const artifacts = await response.json();
        const list = document.getElementById('artifactsList');

        if (artifacts.length === 0) {
            list.innerHTML = '<div class="empty-state"><p>No artifacts yet. Upload the first one!</p></div>';
            return;
        }

        list.innerHTML = artifacts.map(artifact => `
            <div class="card">
                <h3>${escapeHtml(artifact.name)}</h3>
                <p>${escapeHtml(artifact.description)}</p>
                ${artifact.category ? `<span class="badge">📁 ${escapeHtml(artifact.category)}</span>` : ''}
                ${artifact.culture ? `<span class="badge">🌍 ${escapeHtml(artifact.culture)}</span>` : ''}
                ${artifact.estimatedAge ? `<p><strong>Age:</strong> ${escapeHtml(artifact.estimatedAge)}</p>` : ''}
                ${artifact.materials ? `<p><strong>Materials:</strong> ${escapeHtml(artifact.materials)}</p>` : ''}
                ${artifact.historicalContext ? `<p><strong>Context:</strong> ${escapeHtml(artifact.historicalContext.substring(0, 80))}...</p>` : ''}
                <div class="meta">
                    <p>Location: ${artifact.location || 'Not specified'}</p>
                    <p>Added: ${new Date(artifact.createdAt).toLocaleDateString()}</p>
                </div>
                <div class="actions">
                    <button class="btn btn-secondary btn-small" onclick="editArtifact(${artifact.id})">Edit</button>
                    <button class="btn btn-danger btn-small" onclick="deleteArtifact(${artifact.id})">Delete</button>
                </div>
            </div>
        `).join('');
    } catch (error) {
        console.error('Error loading artifacts:', error);
    }
}

async function addArtifact() {
    const name = document.getElementById('artifactName').value;
    const description = document.getElementById('artifactDescription').value;
    const category = document.getElementById('artifactCategory').value;
    const culture = document.getElementById('artifactCulture').value;
    const estimatedAge = document.getElementById('artifactAge').value;
    const materials = document.getElementById('artifactMaterials').value;
    const location = document.getElementById('artifactLocation').value;
    const historicalContext = document.getElementById('artifactContext').value;

    if (!name || !description) {
        alert('Please fill in required fields');
        return;
    }

    try {
        const response = await fetch(`${API_BASE}/artifacts`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                name, description, category, culture, estimatedAge, materials, location, historicalContext,
                contributorId: 1
            })
        });

        if (response.ok) {
            document.getElementById('artifactForm').reset();
            await loadArtifacts();
            await loadStats();
            alert('Artifact uploaded successfully!');
        }
    } catch (error) {
        console.error('Error adding artifact:', error);
        alert('Error uploading artifact');
    }
}

async function deleteArtifact(id) {
    if (!confirm('Are you sure you want to delete this artifact?')) return;

    try {
        const response = await fetch(`${API_BASE}/artifacts/${id}`, { method: 'DELETE' });
        if (response.ok) {
            await loadArtifacts();
            await loadStats();
            alert('Artifact deleted successfully!');
        }
    } catch (error) {
        console.error('Error deleting artifact:', error);
    }
}

// Languages
async function loadLanguages() {
    try {
        const response = await fetch(`${API_BASE}/languages`);
        const entries = await response.json();
        const list = document.getElementById('languagesList');

        if (entries.length === 0) {
            list.innerHTML = '<div class="empty-state"><p>No language entries yet. Add the first one!</p></div>';
            return;
        }

        list.innerHTML = entries.map(entry => `
            <div class="card">
                <h3>${escapeHtml(entry.word)}</h3>
                <p><strong>${escapeHtml(entry.language)}</strong></p>
                <p><strong>Translation:</strong> ${escapeHtml(entry.translation)}</p>
                ${entry.ipa ? `<p><strong>IPA:</strong> ${escapeHtml(entry.ipa)}</p>` : ''}
                ${entry.partOfSpeech ? `<span class="badge">📖 ${escapeHtml(entry.partOfSpeech)}</span>` : ''}
                ${entry.dialect ? `<span class="badge">🗺️ ${escapeHtml(entry.dialect)}</span>` : ''}
                ${entry.example ? `<p><em>"${escapeHtml(entry.example)}"</em></p>` : ''}
                <div class="meta">
                    <p>Added: ${new Date(entry.createdAt).toLocaleDateString()}</p>
                </div>
                <div class="actions">
                    <button class="btn btn-secondary btn-small" onclick="editLanguage(${entry.id})">Edit</button>
                    <button class="btn btn-danger btn-small" onclick="deleteLanguage(${entry.id})">Delete</button>
                </div>
            </div>
        `).join('');
    } catch (error) {
        console.error('Error loading languages:', error);
    }
}

async function addLanguageEntry() {
    const language = document.getElementById('languageName').value;
    const word = document.getElementById('languageWord').value;
    const translation = document.getElementById('languageTranslation').value;
    const ipa = document.getElementById('languageIPA').value;
    const partOfSpeech = document.getElementById('languagePOS').value;
    const example = document.getElementById('languageExample').value;
    const exampleTranslation = document.getElementById('languageExampleTranslation').value;
    const dialect = document.getElementById('languageDialect').value;
    const notes = document.getElementById('languageNotes').value;

    if (!language || !word || !translation) {
        alert('Please fill in required fields');
        return;
    }

    try {
        const response = await fetch(`${API_BASE}/languages`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                language, word, translation, ipa, partOfSpeech, example, exampleTranslation: exampleTranslation, dialect, notes,
                contributorId: 1
            })
        });

        if (response.ok) {
            document.getElementById('languageForm').reset();
            await loadLanguages();
            await loadStats();
            alert('Language entry added successfully!');
        }
    } catch (error) {
        console.error('Error adding language entry:', error);
        alert('Error adding language entry');
    }
}

async function deleteLanguage(id) {
    if (!confirm('Are you sure you want to delete this entry?')) return;

    try {
        const response = await fetch(`${API_BASE}/languages/${id}`, { method: 'DELETE' });
        if (response.ok) {
            await loadLanguages();
            await loadStats();
            alert('Entry deleted successfully!');
        }
    } catch (error) {
        console.error('Error deleting language entry:', error);
    }
}

// Events
async function loadEvents() {
    try {
        const response = await fetch(`${API_BASE}/events`);
        const events = await response.json();
        const list = document.getElementById('eventsList');

        if (events.length === 0) {
            list.innerHTML = '<div class="empty-state"><p>No cultural events yet. Create the first one!</p></div>';
            return;
        }

        list.innerHTML = events.map(event => `
            <div class="card">
                <h3>${escapeHtml(event.title)}</h3>
                <p>${escapeHtml(event.description)}</p>
                ${event.culture ? `<span class="badge">🌍 ${escapeHtml(event.culture)}</span>` : ''}
                ${event.type ? `<span class="badge">📍 ${escapeHtml(event.type)}</span>` : ''}
                <p><strong>Start:</strong> ${new Date(event.startDate).toLocaleDateString()}</p>
                ${event.endDate ? `<p><strong>End:</strong> ${new Date(event.endDate).toLocaleDateString()}</p>` : ''}
                ${event.location ? `<p><strong>Location:</strong> ${escapeHtml(event.location)}</p>` : ''}
                <p><strong>Expected Attendees:</strong> ${event.attendees}</p>
                ${event.isRecurring ? `<p><strong>Recurring:</strong> ${escapeHtml(event.recurrencePattern)}</p>` : ''}
                <div class="meta">
                    <p>Created: ${new Date(event.createdAt).toLocaleDateString()}</p>
                </div>
                <div class="actions">
                    <button class="btn btn-secondary btn-small" onclick="editEvent(${event.id})">Edit</button>
                    <button class="btn btn-danger btn-small" onclick="deleteEvent(${event.id})">Delete</button>
                </div>
            </div>
        `).join('');
    } catch (error) {
        console.error('Error loading events:', error);
    }
}

async function addEvent() {
    const title = document.getElementById('eventTitle').value;
    const description = document.getElementById('eventDescription').value;
    const culture = document.getElementById('eventCulture').value;
    const type = document.getElementById('eventType').value;
    const startDate = document.getElementById('eventStartDate').value;
    const endDate = document.getElementById('eventEndDate').value;
    const location = document.getElementById('eventLocation').value;
    const isRecurring = document.getElementById('eventRecurring').checked;
    const recurrencePattern = document.getElementById('eventRecurrence').value;
    const attendees = parseInt(document.getElementById('eventAttendees').value) || 0;

    if (!title || !description || !startDate) {
        alert('Please fill in required fields');
        return;
    }

    try {
        const response = await fetch(`${API_BASE}/events`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                title, description, culture, type, startDate, endDate, location, isRecurring, recurrencePattern, attendees,
                contributorId: 1
            })
        });

        if (response.ok) {
            document.getElementById('eventForm').reset();
            await loadEvents();
            await loadStats();
            alert('Event created successfully!');
        }
    } catch (error) {
        console.error('Error adding event:', error);
        alert('Error creating event');
    }
}

async function deleteEvent(id) {
    if (!confirm('Are you sure you want to delete this event?')) return;

    try {
        const response = await fetch(`${API_BASE}/events/${id}`, { method: 'DELETE' });
        if (response.ok) {
            await loadEvents();
            await loadStats();
            alert('Event deleted successfully!');
        }
    } catch (error) {
        console.error('Error deleting event:', error);
    }
}

// Utility function for XSS protection
function escapeHtml(text) {
    const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };
    return text.replace(/[&<>"']/g, m => map[m]);
}

// Placeholder functions for edit (can be expanded)
function editStory(id) { alert('Edit functionality coming soon!'); }
function editArtifact(id) { alert('Edit functionality coming soon!'); }
function editLanguage(id) { alert('Edit functionality coming soon!'); }
function editEvent(id) { alert('Edit functionality coming soon!'); }
