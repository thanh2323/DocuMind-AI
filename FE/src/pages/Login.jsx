import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { authService } from '../services/auth.service';

const LoginPage = () => {
    const navigate = useNavigate();
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setLoading(true);

        try {
            await authService.login(email, password);
            // TODO: Redirect to home or dashboard
            navigate('/dashboard');
        } catch (err) {
            console.error("Login Error:", err);
            setError(err.message || "Failed to login. Please check your credentials.");
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="flex min-h-screen w-full bg-background-light dark:bg-background-dark">
            {/* LEFT SIDE (IMAGE) */}
            <div className="hidden lg:block relative w-[40%] h-screen">
                <div
                    className="absolute inset-0 bg-cover bg-center"
                    style={{
                        backgroundImage:
                            "url('https://lh3.googleusercontent.com/aida-public/AB6AXuA4fJaMM0rO9agEa6oID3AmC3IiE-7RjHlAiEfGjXbVziKw30Aae3tyZquiKi2VXFc4HBVxQhzX_RPZil109PhZ5ZgqjTBI2e9AlvJVI68VU1RspxvliwvpPP9_h38viMNepwbBejj6WivMObJA8KWtESMOpRW8zNRNVU-CzHipKqrrqJx3z_pWFg8lticAoVmqI2C_KGjjV2Csn2AqZ2_v-fKsQVFDjUlQf6FmewK_Om73IjBJ-na0enblcvUwUcP4G86K9XvIfn0')",
                    }}
                />
                <div className="absolute inset-0 bg-primary/70 dark:bg-background-dark/80"></div>
            </div>

            {/* RIGHT SIDE (FORM) */}
            <div className="flex flex-1 flex-col justify-center px-6 py-12 lg:px-20 xl:px-24">
                <div className="mx-auto w-full max-w-sm lg:w-96">
                    <div>
                        <div className="flex items-center gap-3 text-primary dark:text-white">
                            <div className="size-8">
                                <svg fill="currentColor" viewBox="0 0 48 48">
                                    <path
                                        clipRule="evenodd"
                                        d="M24 4H42V17.3333V30.6667H24V44H6V30.6667V17.3333H24V4Z"
                                        fillRule="evenodd"
                                    ></path>
                                </svg>
                            </div>
                            <h2 className="text-xl font-bold text-[#0d121b] dark:text-gray-200">
                                AI Smart Document Assistant
                            </h2>
                        </div>

                        <h1 className="text-[#0d121b] dark:text-white text-[32px] font-bold mt-10">
                            Welcome Back
                        </h1>
                        <p className="text-gray-600 dark:text-gray-400 mt-1">
                            Access your smart document assistant.
                        </p>
                    </div>

                    {/* FORM */}
                    <form className="mt-8 space-y-6" onSubmit={handleSubmit}>
                        <div>
                            <label
                                htmlFor="email"
                                className="block text-sm font-medium dark:text-gray-300"
                            >
                                Email address
                            </label>
                            <input
                                id="email"
                                type="email"
                                required
                                value={email}
                                onChange={(e) => setEmail(e.target.value)}
                                placeholder="you@example.com"
                                className="mt-2 w-full h-12 rounded-lg border border-gray-300 dark:border-gray-700 bg-background-light dark:bg-background-dark px-4 text-[#0d121b] dark:text-gray-200 focus:ring-2 focus:ring-primary/50"
                            />
                        </div>

                        <div>
                            <label
                                htmlFor="password"
                                className="block text-sm font-medium dark:text-gray-300"
                            >
                                Password
                            </label>
                            <div className="mt-2 relative">
                                <input
                                    id="password"
                                    type="password"
                                    required
                                    value={password}
                                    onChange={(e) => setPassword(e.target.value)}
                                    placeholder="Enter your password"
                                    className="w-full h-12 rounded-lg border border-gray-300 dark:border-gray-700 bg-background-light dark:bg-background-dark px-4 pr-10 text-[#0d121b] dark:text-gray-200 focus:ring-2 focus:ring-primary/50"
                                />
                                <button
                                    type="button"
                                    className="absolute inset-y-0 right-3 flex items-center text-gray-500 dark:hover:text-gray-300"
                                >
                                    <span className="material-symbols-outlined">visibility_off</span>
                                </button>
                            </div>
                        </div>

                        <div className="flex justify-end">
                            <a className="text-sm font-semibold text-primary hover:text-primary/80">
                                Forgot password?
                            </a>
                        </div>

                        {error && (
                            <div className="flex items-center gap-x-2 rounded-lg bg-red-50 p-3 text-sm text-red-600 dark:bg-red-950/30 dark:text-red-400" role="alert">
                                <span className="material-symbols-outlined" style={{ fontSize: '20px' }}>error</span>
                                <p>{error}</p>
                            </div>
                        )}

                        <button
                            type="submit"
                            disabled={loading}
                            className="w-full h-12 bg-primary text-white rounded-lg font-semibold hover:bg-primary/90 disabled:opacity-50"
                        >
                            {loading ? 'Logging in...' : 'Log in'}
                        </button>
                    </form>

                    <p className="mt-10 text-center text-sm text-gray-500">
                        Don't have an account?{" "}
                        <Link to="/register" className="font-semibold text-primary hover:text-primary/80">Sign up</Link>
                    </p>
                </div>
            </div>
        </div>
    );
};

export default LoginPage;
