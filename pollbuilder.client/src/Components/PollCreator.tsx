import React, { useState } from 'react';

export default function PollCreator() {
    const [question, setQuestion] = useState('');
    const [type, setType] = useState(0); // 0=MCQ, 1=YesNo, 2=Rating, 3=OpenText
    const [options, setOptions] = useState(['', '']);
    const [expiryMinutes, setExpiryMinutes] = useState('');
    const [createdLink, setCreatedLink] = useState('');

    const handleAddOption = () => setOptions([...options, '']);
    const handleRemoveOption = (index: number) => setOptions(options.filter((_, i) => i !== index));

    const handleCreate = async () => {
        const expiresAt = expiryMinutes ? new Date(Date.now() + parseInt(expiryMinutes) * 60000).toISOString() : null;
        const filteredOptions = type === 0 ? options.filter(o => o.trim() !== '') : [];

        const res = await fetch('https://localhost:7168/api/polls', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ question, type, options: filteredOptions, expiresAt })
        });

        if (res.ok) {
            const data = await res.json();
            setCreatedLink(`${window.location.origin}/poll/${data.code}`);
        }
    };

    return (
        <div className="max-w-lg mx-auto bg-white p-6 rounded-xl border border-slate-200 shadow-sm space-y-5">
            <h2 className="text-xl font-bold text-slate-800">Create a New Live Poll</h2>

            <div>
                <label className="block text-sm font-medium text-slate-600 mb-1">Question</label>
                <input className="w-full px-3 py-2 border rounded-lg focus:outline-indigo-500" value={question} onChange={e => setQuestion(e.target.value)} placeholder="What do you want to ask?" />
            </div>

            <div>
                <label className="block text-sm font-medium text-slate-600 mb-1">Poll Type</label>
                <select className="w-full px-3 py-2 border rounded-lg" value={type} onChange={e => setType(parseInt(e.target.value))}>
                    <option value={0}>Multiple Choice</option>
                    <option value={1}>Yes / No</option>
                    <option value={2}>Star Rating (1-5)</option>
                    <option value={3}>Open Text Feedback</option>
                </select>
            </div>

            {type === 0 && (
                <div className="space-y-2">
                    <label className="block text-sm font-medium text-slate-600">Answer Options</label>
                    {options.map((opt, i) => (
                        <div key={i} className="flex gap-2">
                            <input className="flex-1 px-3 py-1.5 border rounded-lg" value={opt} onChange={e => {
                                const updated = [...options]; updated[i] = e.target.value; setOptions(updated);
                            }} placeholder={`Option ${i + 1}`} />
                            {options.length > 2 && <button className="text-red-500 px-2" onClick={() => handleRemoveOption(i)}>✕</button>}
                        </div>
                    ))}
                    <button className="text-sm text-indigo-600 font-semibold" onClick={handleAddOption}>+ Add Option</button>
                </div>
            )}

            <div>
                <label className="block text-sm font-medium text-slate-600 mb-1">Auto-Close Expiry (Minutes from now - optional)</label>
                <input type="number" className="w-full px-3 py-2 border rounded-lg" value={expiryMinutes} onChange={e => setExpiryMinutes(e.target.value)} placeholder="Never expires" />
            </div>

            <button className="w-full bg-indigo-600 hover:bg-indigo-700 text-white font-medium py-2 rounded-lg transition" onClick={handleCreate}>Build Live Poll</button>

            {createdLink && (
                <div className="p-4 bg-emerald-50 text-emerald-900 border border-emerald-200 rounded-lg space-y-1">
                    <p className="text-xs font-semibold uppercase tracking-wider text-emerald-700">Poll Ready!</p>
                    <a className="block text-sm font-mono break-all underline text-indigo-600" href={createdLink} target="_blank" rel="noreferrer">{createdLink}</a>
                </div>
            )}
        </div>
    );
}