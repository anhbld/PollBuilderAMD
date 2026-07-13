import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';

export default function PollVoter() {
    const { code } = useParams();
    const navigate = useNavigate();
    const [poll, setPoll] = useState<any>(null);
    const [selectedOpt, setSelectedOpt] = useState<number | null>(null);
    const [textVal, setTextVal] = useState('');
    const [err, setErr] = useState('');

    // Local storage token serves as a lightweight device fingerprint
    const getVoterToken = () => {
        let token = localStorage.getItem('voter_fingerprint');
        if (!token) {
            token = 'vtr_' + Math.random().toString(36).substring(2, 15);
            localStorage.setItem('voter_fingerprint', token);
        }
        return token;
    };

    useEffect(() => {
        fetch(`https://localhost:7168/api/polls/${code}`)
            .then(res => res.json())
            .then(data => setPoll(data))
            .catch(() => setErr('Poll not found.'));
    }, [code]);

    const handleSubmitVote = async () => {
        const res = await fetch(`https://localhost:7168/api/polls/${code}/vote`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                optionIndex: poll.type === 2 ? selectedOpt : selectedOpt,
                textResponse: poll.type === 3 ? textVal : '',
                voterToken: getVoterToken()
            })
        });

        if (res.ok) {
            navigate(`/poll/${code}/results`);
        } else {
            const msg = await res.text();
            setErr(msg || 'Voting execution failed.');
        }
    };

    if (err) return <div className="text-center text-red-600 p-8 font-medium">{err}</div>;
    if (!poll) return <div className="text-center text-slate-400 p-8">Loading dynamic layout...</div>;
    if (poll.isClosed) return <div className="text-center p-8 text-slate-700 bg-amber-50 rounded-xl border border-amber-200">This poll has concluded. <button className="underline text-indigo-600 ml-2" onClick={() => navigate(`/poll/${code}/results`)}>View Results</button></div>;

    return (
        <div className="max-w-lg mx-auto bg-white p-6 rounded-xl border border-slate-200 shadow-sm space-y-6">
            <h2 className="text-2xl font-bold text-slate-800">{poll.question}</h2>

            {poll.type === 0 && (
                <div className="space-y-2">
                    {poll.options.map((opt: string, i: number) => (
                        <button key={i} onClick={() => setSelectedOpt(i)} className={`w-full text-left p-3 rounded-lg border text-sm font-medium transition ${selectedOpt === i ? 'border-indigo-600 bg-indigo-50 text-indigo-900' : 'border-slate-200 hover:bg-slate-50'}`}>{opt}</button>
                    ))}
                </div>
            )}

            {poll.type === 1 && (
                <div className="grid grid-cols-2 gap-3">
                    <button onClick={() => setSelectedOpt(0)} className={`p-4 text-center rounded-lg border font-bold transition ${selectedOpt === 0 ? 'bg-emerald-50 border-emerald-500 text-emerald-900' : 'border-slate-200'}`}>Yes</button>
                    <button onClick={() => setSelectedOpt(1)} className={`p-4 text-center rounded-lg border font-bold transition ${selectedOpt === 1 ? 'bg-red-50 border-red-500 text-red-900' : 'border-slate-200'}`}>No</button>
                </div>
            )}

            {poll.type === 2 && (
                <div className="flex justify-center gap-2 text-3xl">
                    {[1, 2, 3, 4, 5].map((star) => (
                        <button key={star} onClick={() => setSelectedOpt(star)} className={selectedOpt && selectedOpt >= star ? 'text-amber-400' : 'text-slate-200'}>★</button>
                    ))}
                </div>
            )}

            {poll.type === 3 && (
                <textarea className="w-full p-3 border rounded-lg focus:outline-indigo-500 text-sm" rows={4} value={textVal} onChange={e => setTextVal(e.target.value)} placeholder="Type your context here..." />
            )}

            <button className="w-full bg-slate-900 hover:bg-slate-800 text-white font-medium py-2.5 rounded-lg transition" onClick={handleSubmitVote}>Submit Response</button>
        </div>
    );
}