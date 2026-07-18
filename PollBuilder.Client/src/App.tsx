import { BrowserRouter, Routes, Route } from 'react-router-dom';
// 🟢 Update these lines to use capital "Components"
import PollCreator from './Components/PollCreator';
import PollVoter from './Components/PollVoter';
import PollResults from './Components/PollResults';

function App() {
    return (
        <BrowserRouter>
            <div className="min-h-screen bg-slate-900 text-slate-100 selection:bg-indigo-500">
                <main className="container mx-auto px-4 py-8 max-w-4xl">
                    <Routes>
                        {/* 1. Base route loads the creation panel */}
                        <Route path="/" element={<PollCreator />} />

                        {/* 2. Shortcode path displays the voter panel */}
                        <Route path="/poll/:code" element={<PollVoter />} />

                        {/* 3. Results path exposes live SignalR charts */}
                        <Route path="/poll/:code/results" element={<PollResults />} />
                    </Routes>
                </main>
            </div>
        </BrowserRouter>
    );
}

export default App;