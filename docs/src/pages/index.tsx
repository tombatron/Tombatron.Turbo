import React from 'react';
import clsx from 'clsx';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import Layout from '@theme/Layout';
import CodeBlock from '@theme/CodeBlock';

import styles from './index.module.css';

function HeroSection() {
  const {siteConfig} = useDocusaurusContext();
  return (
    <header className="hero--turbo">
      <div className="container">
        <h1>Hotwire Turbo for ASP.NET Core</h1>
        <p>
          Build fast, modern web apps with server-rendered HTML.
          No JavaScript framework needed.
        </p>
        <div className="hero__buttons">
          <Link className="hero__button--primary" to="/docs/getting-started">
            Get Started
          </Link>
          <Link
            className="hero__button--outline"
            to="https://github.com/tombatron/Tombatron.Turbo">
            View on GitHub
          </Link>
        </div>
        <div className="hero__badges">
          <a href="https://www.nuget.org/packages/Tombatron.Turbo/">
            <img
              alt="NuGet"
              src="https://img.shields.io/nuget/v/Tombatron.Turbo.svg?style=flat-square"
            />
          </a>
          <a href="https://www.npmjs.com/package/@tombatron/turbo-signalr">
            <img
              alt="npm"
              src="https://img.shields.io/npm/v/@tombatron/turbo-signalr.svg?style=flat-square"
            />
          </a>
          <a href="https://github.com/tombatron/Tombatron.Turbo/actions/workflows/build.yml">
            <img
              alt="Build"
              src="https://img.shields.io/github/actions/workflow/status/tombatron/Tombatron.Turbo/build.yml?style=flat-square"
            />
          </a>
        </div>
      </div>
    </header>
  );
}

const features = [
  {
    icon: '🖼',
    title: 'Partial Page Updates',
    description:
      'Turbo Frames give you SPA-like speed with server-rendered HTML. Wrap a section in a frame and only that section updates on navigation.',
  },
  {
    icon: '📡',
    title: 'Real-Time via SignalR',
    description:
      'Push live updates to any element with Turbo Streams. Append, replace, remove — all over WebSocket with automatic reconnection.',
  },
  {
    icon: '🔒',
    title: 'Compile-Time Safety',
    description:
      'The bundled source generator creates strongly-typed partial references. Catch typos at build time, not in production.',
  },
  {
    icon: '⚡',
    title: 'Zero Configuration',
    description:
      'AddTurbo() + MapTurboHub() and you\'re running. Turbo.js, SignalR bridge, and signed stream security are all wired up automatically.',
  },
  {
    icon: '🧩',
    title: 'Familiar Patterns',
    description:
      'Tag helpers, Razor partials, dependency injection — everything you already know. No new paradigms to learn.',
  },
  {
    icon: '🎯',
    title: 'Stimulus Integration',
    description:
      'Convention-based controller discovery with import maps. Drop a file in wwwroot/controllers/ and it just works, with hot reload in development.',
  },
];

function FeaturesSection() {
  return (
    <section className="features">
      <h2>Why Tombatron.Turbo?</h2>
      <div className="features__grid">
        {features.map((feature, idx) => (
          <div key={idx} className="feature-card">
            <div className="feature-card__icon">{feature.icon}</div>
            <h3>{feature.title}</h3>
            <p>{feature.description}</p>
          </div>
        ))}
      </div>
    </section>
  );
}

function CodeShowcase() {
  return (
    <section className="code-showcase">
      <div className="container">
        <h2>Up and Running in Minutes</h2>
        <p>Three lines to set up, a tag helper for frames, two lines for real-time.</p>
        <div className="code-showcase__grid">
          <div className="code-showcase__item">
            <h3>Setup</h3>
            <CodeBlock language="csharp">
{`builder.Services.AddTurbo();
app.UseTurbo();
app.MapTurboHub();`}
            </CodeBlock>
          </div>
          <div className="code-showcase__item">
            <h3>Turbo Frame</h3>
            <CodeBlock language="html">
{`<turbo-frame id="cart" src="/cart"
             loading="lazy">
  Loading...
</turbo-frame>`}
            </CodeBlock>
          </div>
          <div className="code-showcase__item">
            <h3>Real-Time Broadcast</h3>
            <CodeBlock language="csharp">
{`await _turbo.Stream("notifications", b =>
    b.Append("list", "<div>New!</div>"));`}
            </CodeBlock>
          </div>
        </div>
      </div>
    </section>
  );
}

function CTASection() {
  return (
    <section className="cta-section">
      <h2>Ready to get started?</h2>
      <Link className="hero__button--primary" to="/docs/getting-started">
        Read the Quickstart
      </Link>
    </section>
  );
}

export default function Home(): React.JSX.Element {
  const {siteConfig} = useDocusaurusContext();
  return (
    <Layout
      title="Hotwire Turbo for ASP.NET Core"
      description="Build fast, modern web apps with server-rendered HTML and SignalR-powered real-time streams.">
      <HeroSection />
      <main>
        <FeaturesSection />
        <CodeShowcase />
        <CTASection />
      </main>
    </Layout>
  );
}
