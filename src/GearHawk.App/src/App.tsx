import { useState } from 'react'
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom'
import { useAppDispatch, useAppSelector } from './store/hooks'
import { logout } from './store/slices/authSlice'
import { increment, decrement, reset } from './store/slices/counterSlice'
import { useLoginMutation, useGetUserProfileQuery } from './store/api/gearHawkApi'
import { useMsal } from '@azure/msal-react'
import { InteractionRequiredAuthError, InteractionType } from '@azure/msal-browser'
import { loginRequest, protectedResources } from './config/authConfig'
import './App.css'

function App() {
  const dispatch = useAppDispatch()
  const { isAuthenticated, user, loading, error } = useAppSelector((state) => state.auth)
  const { count } = useAppSelector((state) => state.counter)
  
  const [login] = useLoginMutation()
  const { instance, accounts } = useMsal()
  const isMsalAuthenticated = accounts.length > 0
  const { data: profileData } = useGetUserProfileQuery(undefined, { skip: !isMsalAuthenticated })

  const handleLogin = async () => {
    try {
      const active = accounts[0]
      if (!active) {
        await instance.loginRedirect(loginRequest as any)
      }
    } catch (error) {
      console.error('Login failed:', error)
    }
  }

  const handleLogout = () => {
    instance.logoutRedirect({ postLogoutRedirectUri: window.location.origin })
    dispatch(logout())
  }

  return (
    <Router>
      <div className="App">
        <header className="App-header">
          <h1>GearHawk App</h1>
          <nav>
            <Link to="/">Home</Link>
            <Link to="/counter">Counter</Link>
            <Link to="/profile">Profile</Link>
          </nav>
          <div className="auth-section">
            {isMsalAuthenticated ? (
              <div>
                <span>Welcome, {user?.name || (profileData as any)?.name || accounts[0]?.name || 'User'}!</span>
                <button onClick={handleLogout}>Logout</button>
              </div>
            ) : (
              <button onClick={handleLogin} disabled={loading}>
                {loading ? 'Logging in...' : 'Login with Microsoft Entra'}
              </button>
            )}
            {error && <div className="error">{error}</div>}
          </div>
        </header>

        <main>
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/counter" element={<Counter />} />
            <Route path="/profile" element={<Profile />} />
          </Routes>
        </main>
      </div>
    </Router>
  )
}

function Home() {
  return (
    <div className="home">
      <h2>Hello World from GearHawk!</h2>
      <p>Welcome to your new React application with Redux Toolkit and RTK Query.</p>
      <div className="features">
        <h3>Features included:</h3>
        <ul>
          <li>✅ Vite for fast development</li>
          <li>✅ TypeScript for type safety</li>
          <li>✅ Redux Toolkit for state management</li>
          <li>✅ RTK Query for API caching</li>
          <li>✅ React Router for navigation</li>
          <li>✅ Microsoft Entra authentication ready</li>
          <li>✅ Axios for API calls</li>
        </ul>
      </div>
    </div>
  )
}

function Counter() {
  const dispatch = useAppDispatch()
  const { count } = useAppSelector((state) => state.counter)

  return (
    <div className="counter">
      <h2>Counter Example</h2>
      <p>This demonstrates Redux Toolkit state management:</p>
      <div className="counter-display">
        <h3>Count: {count}</h3>
        <div className="counter-buttons">
          <button onClick={() => dispatch(decrement())}>-</button>
          <button onClick={() => dispatch(reset())}>Reset</button>
          <button onClick={() => dispatch(increment())}>+</button>
        </div>
      </div>
    </div>
  )
}

function Profile() {
  const { accounts } = useMsal()
  const isMsalAuthenticated = accounts.length > 0
  const { data: profileData, isLoading, error } = useGetUserProfileQuery(undefined, {
    skip: !isMsalAuthenticated,
  })

  if (!isMsalAuthenticated) {
    return (
      <div className="profile">
        <h2>Profile</h2>
        <p>Please log in to view your profile.</p>
      </div>
    )
  }

  if (isLoading) {
    return (
      <div className="profile">
        <h2>User Profile</h2>
        <p>Loading profile...</p>
      </div>
    )
  }

  if (error) {
    return (
      <div className="profile">
        <h2>User Profile</h2>
        <p>Error loading profile: {JSON.stringify(error)}</p>
      </div>
    )
  }

  const displayUser = profileData || { name: accounts[0]?.name, email: accounts[0]?.username, id: accounts[0]?.homeAccountId }

  return (
    <div className="profile">
      <h2>User Profile</h2>
      <div className="profile-info">
        <p><strong>Name:</strong> {displayUser?.name || 'Not available'}</p>
        <p><strong>Email:</strong> {displayUser?.email || 'Not available'}</p>
        <p><strong>ID:</strong> {displayUser?.id || 'Not available'}</p>
      </div>
    </div>
  )
}

export default App
