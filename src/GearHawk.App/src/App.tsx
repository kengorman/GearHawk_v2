import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom'
import InventoryHome from './pages/InventoryHome'
import { useAppDispatch, useAppSelector } from './store/hooks'
import { logout } from './store/slices/authSlice'
import { increment, decrement, reset } from './store/slices/counterSlice'
import { useLoginMutation, useGetUserProfileQuery } from './store/api/gearHawkApi'
import { useMsal } from '@azure/msal-react'
import { loginRequest } from './config/authConfig'
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
        <header className="site-header">
          <div className="container header-inner">
            <div className="brand">
              <span className="brand-logo" aria-hidden="true"></span>
              <span className="brand-name">Gear Hawk</span>
            </div>
            <nav className="primary-nav" aria-label="Primary">
              <Link to="/">Home</Link>
              <Link to="/counter">Counter</Link>
              <Link to="/profile">Profile</Link>
            </nav>
            <div className="auth-actions">
              {isMsalAuthenticated ? (
                <>
                  <span className="visually-hidden">Signed in as </span>
                  <span>{user?.name || (profileData as any)?.name || accounts[0]?.name || 'User'}</span>
                  <button className="btn" onClick={handleLogout}>Logout</button>
                </>
              ) : (
                <button className="btn primary" onClick={handleLogin} disabled={loading}>
                  {loading ? 'Signing in…' : 'Sign in'}
                </button>
              )}
            </div>
          </div>
        </header>

        <main>
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/counter" element={<Counter />} />
            <Route path="/profile" element={<Profile />} />
            <Route path="/inventory/:id?" element={<InventoryHome />} />
          </Routes>
        </main>

        <footer className="site-footer">
          <div className="container">
            <small>© {new Date().getFullYear()} Gear Hawk. All rights reserved.</small>
          </div>
        </footer>
      </div>
    </Router>
  )
}

function Home() {
  return (
    <>
      <section className="hero">
        <div className="container">
          <span className="hero-eyebrow">Mobile-first</span>
          <h1 className="hero-title">Manage gear and rig checks with speed and confidence</h1>
          <p className="hero-subtitle">
            Gear Hawk brings inventory, inspections, and reporting into one streamlined workflow.
          </p>
          <div className="hero-cta">
            <Link className="btn primary" to="/inventory/-1">Get started</Link>
            <Link className="btn" to="/counter">Try the demo</Link>
          </div>
        </div>
      </section>

      <section className="features">
        <div className="container">
          <div className="feature-grid">
            <article className="feature-card">
              <h3>Fast inventory</h3>
              <p>Quickly add, find, and update gear from any device.</p>
            </article>
            <article className="feature-card">
              <h3>Rig checks</h3>
              <p>Standardized checklists ensure safety and compliance.</p>
            </article>
            <article className="feature-card">
              <h3>Reports</h3>
              <p>Share real-time insights with your team and stakeholders.</p>
            </article>
          </div>
        </div>
      </section>
    </>
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
